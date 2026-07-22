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

// Shared machinery for the two independently-stacking Standing By powers: BalancedPower (random,
// the base card) and BalancedChoicePower (player choice, the upgraded card). They stack as
// separate power types, so a creature can hold both at once — one freeing random cards, the other
// freeing chosen cards. Both present as the same "Balanced" badge (name + icon); they differ
// only in SelectCards and their tooltip fragment.
//
// Amount = how many hand cards this power frees PER trigger. Whenever a card becomes Unplayable,
// each present Standing By power independently frees up to its own Amount eligible cards in hand.
public abstract class BalancedPowerBase : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Both variants show one shared "Balanced" icon regardless of subclass name, rather than each
    // deriving a different (art-less) path from its own Id.Entry. No dedicated art exists yet, so
    // both currently fall back to the game's missing_power glyph — this just keeps them identical.
    public override string? CustomPackedIconPath => "standingby.png".PowerImagePath();
    public override string? CustomBigIconPath => "standingby.png".BigPowerImagePath();

    // The tooltip fragment describing HOW a freed card is picked, spliced into the shared text.
    protected abstract string SelectionFragment { get; }

    public override List<(string, string)> Localization => new PowerLoc(
        "Balanced",
        $"Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 {SelectionFragment} in hand.",
        $"Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from {{Amount}} {SelectionFragment} in hand.");

    // Behind InitInternalData rather than an ordinary field: a power reaches combat through
    // ToMutable() -> MutableClone(), a MemberwiseClone that copies reference fields BY REFERENCE, so
    // a plain List would stay shared with the canonical model — and therefore between every creature
    // carrying this power, which in multiplayer means two players queueing into one list. Only
    // PowerModel.DeepCloneFields re-isolates state, and the only thing it re-creates is _internalData
    // (base-game OrbitPower is the reference implementation).
    private sealed class Data
    {
        public readonly List<CardModel> Pending = new();
    }

    protected override object InitInternalData() => new Data();

    private List<CardModel> Pending => GetInternalData<Data>().Pending;

    // Defensive reset, called every time a Standing By card is played (see Balanced.OnPlay).
    // PowerCmd.Apply reuses an existing same-type Power instance for stacking rather than always
    // constructing fresh, so a leftover instance from a retried/reloaded combat could otherwise act
    // on stale queued state in the new attempt. This power is not Instanced, so unlike
    // SecondLessonPower it really can be handed back a reused instance; clearing only its own queue
    // (no board-visible state) is safe to redo on every play.
    public void ResetTracking() => Pending.Clear();

    // AfterApplied fires on EVERY application — including each stack onto this same reused instance
    // (PowerCmd reuses a same-type instance for stacking, see ResetTracking). Subscribe idempotently
    // (unsubscribe first) so a stacked power queues each Unplayable event exactly once; otherwise the
    // handler would be attached once per stack and free Amount-times too many cards per trigger.
    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
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
        if (card.Owner?.Creature == Owner) Pending.Add(card);
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
    // UnplayableModifier when a card's final Tuned play just completed. The Late pass runs after every
    // listener's AfterCardPlayed has finished, so any Unplayable attached during this same play —
    // including the just-played card's own — has already queued via OnUnplayableApplied above.
    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Pending.Count == 0 || Owner?.Player == null) return;
        var triggers = Pending.ToList();
        Pending.Clear();

        var player = Owner.Player;
        int restBefore = CountUnplayableOutsideHand(player);
        int perTrigger = (int)Amount;

        // Note: a triggering card need NOT still be Unplayable here. The drain is deferred to the
        // Late pass, and between the trigger firing and now the card may have been freed (by this
        // power, by Breather/Unwind/Touch Up, or during a Workshop auto-play chain) — the
        // "whenever a card becomes Unplayable" reaction still stands. We only ever free OTHER cards
        // in hand, guarded by the outside-hand invariant below.
        foreach (var triggeringCard in triggers)
        {
            var candidates = PileType.Hand.GetPile(player).Cards
                .Where(c => c != triggeringCard && UnplayableModifier.CanApplyTo(c))
                .ToList();
            if (candidates.Count == 0 || perTrigger <= 0) continue;

            var chosen = await SelectCards(context, player, candidates, Math.Min(perTrigger, candidates.Count));
            foreach (var card in chosen)
            {
                UnplayableModifier.Remove(card);
                Invariants.Check(!card.TryGetModifier<UnplayableModifier>(out _),
                    nameof(BalancedPowerBase) + "." + nameof(AfterCardPlayedLate),
                    $"{card.Id} was chosen to be freed but still carries UnplayableModifier after Remove");
            }
        }

        int restAfter = CountUnplayableOutsideHand(player);
        Invariants.Check(restAfter >= restBefore,
            nameof(BalancedPowerBase) + "." + nameof(AfterCardPlayedLate),
            $"Unplayable count outside hand decreased ({restBefore} -> {restAfter}) — this power must never " +
            "remove Unplayable from anything but a card in hand");
    }
}
