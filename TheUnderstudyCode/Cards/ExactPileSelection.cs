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

// Shared "select exactly N cards from a combat pile" for the Planned appliers (Workshop, Foreshadow,
// Callback, Magnum Opus). The base-game combat-pile screen (NCombatPileCardSelectScreen) AUTO-COMPLETES on
// min==max (no confirm button), so to get the gated-confirm UX the mod's hand cards get for free we set
// RequireManualConfirmation = true — which in turn disables the engine's auto-apply-when-few shortcut, so we
// do that ourselves here: with count-or-fewer eligible cards, return them all without ever showing the
// screen. Only arms the Planned order badges when the screen will actually show, so the auto-apply path
// can't leave the badge state armed for the next selection.
public static class ExactPileSelection
{
    // Prompt only when there are MORE eligible cards than required; otherwise auto-apply them all.
    public static bool ShouldPrompt(int candidateCount, int required) => candidateCount > required;

    public static async Task<IReadOnlyList<CardModel>> Select(
        PlayerChoiceContext context, CardPile pile, Player player, int count,
        string promptKey, Func<CardModel, bool> filter, bool armPlannedBadges)
    {
        var candidates = pile.Cards.Where(filter).ToList();
        if (!ShouldPrompt(candidates.Count, count)) return candidates; // auto-apply all, no screen

        if (armPlannedBadges && MultiplayerUtil.IsLocalPlayer(player)) PlannedSelectionState.Arm();
        var prefs = new CardSelectorPrefs(new LocString("cards", promptKey), count, count)
        {
            RequireManualConfirmation = true, // gate confirm at exactly N; do NOT auto-complete
        };
        var selected = await CardSelectCmd.FromCombatPile(context, pile, player, prefs, filter);
        return selected?.ToList() ?? new List<CardModel>();
    }
}
