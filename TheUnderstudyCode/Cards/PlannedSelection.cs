using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Selection for applying PLANNED. Planned is order-sensitive — the queue plays in the order the player picks
// — so, unlike Tuned (which auto-applies when there are few candidates), this ALWAYS shows the selection
// screen whenever there's at least one eligible card, letting the player choose which cards and in what
// order. RequireManualConfirmation = true turns OFF the engine's auto-apply-when-few shortcut for both the
// hand (NPlayerHand) and combat-pile (NCombatPileCardSelectScreen) screens. The player must pick exactly
// min(count, eligible) cards — clamped because the hand UI's confirm button has no available-count clamp, so
// asking for more than are in hand would soft-lock. Arms the Planned order badges; the caller applies the
// result via PlannedSelectionState.InClickOrder + PlannedModifier.Apply.
public static class PlannedSelection
{
    // How many the player must pick: the card's amount, clamped to the eligible count so the confirm button
    // is always reachable. Pure / unit-tested.
    public static int RequiredCount(int desired, int eligibleCount) => Math.Min(desired, eligibleCount);

    public static Task<IReadOnlyList<CardModel>> FromHand(
        PlayerChoiceContext context, Player player, int count, string promptKey,
        Func<CardModel, bool> filter, AbstractModel source) =>
        Select(player, PileType.Hand.GetPile(player), count, promptKey,
            prefs => CardSelectCmd.FromHand(context, player, prefs, filter, source), filter);

    public static Task<IReadOnlyList<CardModel>> FromPile(
        PlayerChoiceContext context, CardPile pile, Player player, int count, string promptKey,
        Func<CardModel, bool> filter) =>
        Select(player, pile, count, promptKey,
            prefs => CardSelectCmd.FromCombatPile(context, pile, player, prefs, filter), filter);

    private static async Task<IReadOnlyList<CardModel>> Select(
        Player player, CardPile pile, int count, string promptKey,
        Func<CardSelectorPrefs, Task<IEnumerable<CardModel>>> show, Func<CardModel, bool> filter)
    {
        var candidates = pile.Cards.Where(filter).ToList();
        int required = RequiredCount(count, candidates.Count);
        if (required <= 0) return Array.Empty<CardModel>(); // nothing to plan — no screen, don't arm

        if (MultiplayerUtil.IsLocalPlayer(player)) PlannedSelectionState.Arm();
        var prefs = new CardSelectorPrefs(new LocString("cards", promptKey), required, required)
        {
            RequireManualConfirmation = true, // always manual — never auto-apply order-sensitive Planned
        };
        var selected = await show(prefs);
        return selected?.ToList() ?? new List<CardModel>();
    }
}
