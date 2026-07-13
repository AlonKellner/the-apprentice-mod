using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Shared machinery for the two independently-stacking Standing By powers: StandingByPower (random,
// the base card) and StandingByChoicePower (player choice, the upgraded card). They stack as
// separate power types, so a creature can hold both at once — one freeing random cards, the other
// freeing chosen cards. Both present as the same "Standing By" badge (name + icon); they differ
// only in SelectCards and their tooltip fragment.
//
// Amount = how many hand cards this power frees PER trigger. Whenever a card becomes Unplayable,
// each present Standing By power independently frees up to its own Amount eligible cards in hand.
public abstract class StandingByPowerBase : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Both variants show one shared "Standing By" icon regardless of subclass name, rather than each
    // deriving a different (art-less) path from its own Id.Entry. No dedicated art exists yet, so
    // both currently fall back to the game's missing_power glyph — this just keeps them identical.
    public override string? CustomPackedIconPath => "standingby.png".PowerImagePath();
    public override string? CustomBigIconPath => "standingby.png".BigPowerImagePath();

    // The tooltip fragment describing HOW a freed card is picked, spliced into the shared text.
    protected abstract string SelectionFragment { get; }

    public override List<(string, string)> Localization => new PowerLoc(
        "Standing By",
        $"Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 {SelectionFragment} in hand.",
        $"Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from {{Amount}} {SelectionFragment} in hand.");

    private readonly List<CardModel> _pending = new();

    // Defensive reset, called every time a Standing By card is played (see StandingBy.OnPlay).
    // PowerCmd.Apply reuses an existing same-type Power instance for stacking rather than always
    // constructing fresh, so a leftover instance from a retried/reloaded combat could otherwise act
    // on stale queued state in the new attempt (same fix as SecondLessonPower.ResetTracking).
    public void ResetTracking() => _pending.Clear();

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied += OnUnplayableApplied;
        return Task.CompletedTask;
    }

    // NOT AfterRemoved: Creature.RemoveAllPowersInternalExcept (the bulk wipe at combat end) skips
    // AfterRemoved, so it never fires on a normal combat end. AfterCombatEnd is the reliable hook
    // (same as PlannedCounterPower). Without this, the static subscription lingers into every future
    // combat, freeing Unplayable cards even with Standing By nowhere in play.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
        return Task.CompletedTask;
    }

    private void OnUnplayableApplied(CardModel card)
    {
        if (card.Owner?.Creature == Owner) _pending.Add(card);
    }

    // "The rest of the deck" — every other combat pile. This power only ever removes Unplayable from
    // a card in Hand, so this count must never decrease as a result of its own action.
    private static int CountUnplayableOutsideHand(Player player) =>
        CardPile.GetCards(player, PileType.Draw, PileType.Discard, PileType.Exhaust, PileType.Play)
            .Count(UnplayableModifier.CanApplyTo);

    // Picks up to `count` cards to free from a non-empty candidate list — random (base) or player
    // choice (upgraded). May return fewer than `count` (e.g. the player declines in choice mode).
    protected abstract Task<IReadOnlyList<CardModel>> SelectCards(
        PlayerChoiceContext context, Player player, IReadOnlyList<CardModel> candidates, int count);

    // Must run in the *Late* pass, not the main AfterCardPlayed pass: CombatState.IterateHookListeners
    // enumerates each creature's Powers before its cards, so this Power's own AfterCardPlayed would
    // fire BEFORE the just-played card's own AfterCardPlayed override — the one that attaches
    // UnplayableModifier when a card's final Tense play just completed. The Late pass runs after every
    // listener's AfterCardPlayed has finished, so any Unplayable attached during this same play —
    // including the just-played card's own — has already queued via OnUnplayableApplied above.
    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (_pending.Count == 0 || Owner?.Player == null) return;
        var triggers = _pending.ToList();
        _pending.Clear();

        Invariants.Check(triggers.Distinct().Count() == triggers.Count,
            nameof(StandingByPowerBase) + "." + nameof(AfterCardPlayedLate),
            "the pending trigger queue has duplicate cards — OnUnplayableApplied fired twice for the same card");

        var player = Owner.Player;
        int restBefore = CountUnplayableOutsideHand(player);
        int perTrigger = (int)Amount;

        foreach (var triggeringCard in triggers)
        {
            Invariants.Check(triggeringCard.TryGetModifier<UnplayableModifier>(out _),
                nameof(StandingByPowerBase) + "." + nameof(AfterCardPlayedLate),
                $"{triggeringCard.Id} queued this trigger by becoming Unplayable, but no longer carries UnplayableModifier by drain time");

            var candidates = PileType.Hand.GetPile(player).Cards
                .Where(c => c != triggeringCard && UnplayableModifier.CanApplyTo(c))
                .ToList();
            if (candidates.Count == 0 || perTrigger <= 0) continue;

            var chosen = await SelectCards(context, player, candidates, Math.Min(perTrigger, candidates.Count));
            foreach (var card in chosen)
            {
                UnplayableModifier.Remove(card);
                Invariants.Check(!card.TryGetModifier<UnplayableModifier>(out _),
                    nameof(StandingByPowerBase) + "." + nameof(AfterCardPlayedLate),
                    $"{card.Id} was chosen to be freed but still carries UnplayableModifier after Remove");
            }
        }

        int restAfter = CountUnplayableOutsideHand(player);
        Invariants.Check(restAfter >= restBefore,
            nameof(StandingByPowerBase) + "." + nameof(AfterCardPlayedLate),
            $"Unplayable count outside hand decreased ({restBefore} -> {restAfter}) — this power must never " +
            "remove Unplayable from anything but a card in hand");
    }
}
