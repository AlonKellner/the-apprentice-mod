using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class StandingByPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Set true once an upgraded Standing By has been played this combat. Sticky rather than
    // reset by later unupgraded copies — once the player has access to the smarter version,
    // it stays available.
    public bool ChoiceMode { get; set; }

    // PowerLoc's 2nd/3rd args are (Description, SmartDescription) — the "dumb" tip shown when
    // another card previews this Power vs. the live in-combat tooltip on the actual applied
    // instance — NOT a base/upgraded pair. Both need to reflect the live ChoiceMode here (the
    // card's own cards.json description already handles the base-vs-upgraded preview via
    // {IfUpgraded:show:...}), otherwise the in-combat tooltip would always claim "of your choice"
    // even on an unupgraded (still-random) instance.
    private string DescriptionText => ChoiceMode
        ? "Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 attack or skill of your choice in hand."
        : "Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 random attack or skill in hand.";

    public override List<(string, string)> Localization => new PowerLoc("Standing By", DescriptionText, DescriptionText);

    private readonly List<CardModel> _pending = new();

    // Defensive reset, called every time StandingBy is played (see StandingBy.OnPlay). PowerCmd.Apply
    // reuses an existing same-type Power instance for stacking (FindExistingInstanceForStacking)
    // rather than always constructing fresh — see SecondLessonPower.ResetTracking for the same fix
    // applied there, and the in-game bug it was found from (a combat retried/reloaded after some
    // state had already accumulated on a leftover instance, which then got acted on again in the new
    // attempt). ChoiceMode is intentionally NOT reset — it's meant to stay sticky once unlocked.
    public void ResetTracking() => _pending.Clear();

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied += OnUnplayableApplied;
        return Task.CompletedTask;
    }

    // NOT AfterRemoved: Creature.RemoveAllPowersInternalExcept (the bulk wipe used at combat end)
    // is explicitly documented to skip the AfterRemoved hook for every power it clears, so it never
    // fires on a normal combat end. AfterCombatEnd is the hook PlannedCounterPower already
    // established as the reliable one for this exact cleanup shape. Without this, the static
    // subscription lingers into every future combat, freeing Unplayable cards even with Standing
    // By nowhere in play.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
        return Task.CompletedTask;
    }

    private void OnUnplayableApplied(CardModel card)
    {
        if (card.Owner?.Creature == Owner) _pending.Add(card);
    }

    private static int CountUnplayableInHand(Player player) =>
        PileType.Hand.GetPile(player).Cards.Count(UnplayableModifier.CanApplyTo);

    // "The rest of the deck" — every other combat pile. This power only ever removes Unplayable
    // from a card in Hand, so this count must never decrease as a result of its own action.
    private static int CountUnplayableOutsideHand(Player player) =>
        CardPile.GetCards(player, PileType.Draw, PileType.Discard, PileType.Exhaust, PileType.Play)
            .Count(UnplayableModifier.CanApplyTo);

    // Must run in the *Late* pass, not the main AfterCardPlayed pass: CombatState.IterateHookListeners
    // enumerates each creature's Powers before its cards (see CombatState's hook-listener builder),
    // so this Power's own AfterCardPlayed would fire BEFORE the just-played card's own AfterCardPlayed
    // override — the one that attaches UnplayableModifier when a card's own final Intense play just
    // completed (UnderstudyCard.AfterCardPlayed's IsFinalIntensePlay check). That self-triggered
    // OnUnplayableApplied call would arrive one play too late for a same-pass drain, so the card
    // that JUST became Unplayable by playing itself would only free a hand card on the NEXT play,
    // not this one. AfterCardPlayedLate runs in a second, separate pass after every listener's
    // AfterCardPlayed (cards and powers alike) has already completed, so by the time this fires,
    // any Unplayable attached during this same play — including the just-played card's own — has
    // already fired OnUnplayableApplied above.
    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (_pending.Count == 0 || Owner?.Player == null) return;
        var triggers = _pending.ToList();
        _pending.Clear();

        Invariants.Check(triggers.Distinct().Count() == triggers.Count,
            nameof(StandingByPower) + "." + nameof(AfterCardPlayedLate),
            "the pending trigger queue has duplicate cards — OnUnplayableApplied fired twice for the same card");

        var player = Owner.Player;
        int handBefore = CountUnplayableInHand(player);
        int restBefore = CountUnplayableOutsideHand(player);
        Log.Info($"StandingByPower.AfterCardPlayedLate: draining {triggers.Count} trigger(s), " +
                 $"{handBefore} Unplayable in hand, {restBefore} Unplayable in the rest of the deck");

        foreach (var triggeringCard in triggers)
        {
            Invariants.Check(triggeringCard.TryGetModifier<UnplayableModifier>(out _),
                nameof(StandingByPower) + "." + nameof(AfterCardPlayedLate),
                $"{triggeringCard.Id} queued this trigger by becoming Unplayable, but no longer carries UnplayableModifier by drain time");

            var candidates = PileType.Hand.GetPile(player).Cards
                .Where(c => c != triggeringCard && UnplayableModifier.CanApplyTo(c))
                .ToList();
            if (candidates.Count == 0) continue;

            CardModel? chosen;
            if (ChoiceMode)
            {
                var selected = await CardSelectCmd.FromHand(
                    context,
                    player,
                    new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-STANDING_BY.selectionPrompt"), 0, 1),
                    c => candidates.Contains(c),
                    this);
                // ChoiceMode allows a legitimate decline (CardSelectorPrefs's MinSelect is 0), so
                // chosen == null here is not itself an invariant violation.
                chosen = selected?.FirstOrDefault();
            }
            else
            {
                chosen = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
                Invariants.Check(chosen != null,
                    nameof(StandingByPower) + "." + nameof(AfterCardPlayedLate),
                    "a non-empty candidate list must always yield a pick on the random (non-ChoiceMode) path");
            }

            if (chosen == null) continue;
            UnplayableModifier.Remove(chosen);
            Invariants.Check(!chosen.TryGetModifier<UnplayableModifier>(out _),
                nameof(StandingByPower) + "." + nameof(AfterCardPlayedLate),
                $"{chosen.Id} was chosen to be freed but still carries UnplayableModifier after Remove");
        }

        int restAfter = CountUnplayableOutsideHand(player);
        Invariants.Check(restAfter >= restBefore,
            nameof(StandingByPower) + "." + nameof(AfterCardPlayedLate),
            $"Unplayable count outside hand decreased ({restBefore} -> {restAfter}) — this power must never " +
            "remove Unplayable from anything but a card in hand");
    }
}
