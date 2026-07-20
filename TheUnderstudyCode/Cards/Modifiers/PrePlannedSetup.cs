using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Enchantments;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// Coordinates the once-per-combat assignment of concrete, deck-ordered Planned slots to every
// pre-Planned card. Replaces the old per-card sentinel slot -1 (all pre-Planned cards overlapped at
// -1): now each gets a unique index straight from the normal PlannedSlotSequencer, handed out in
// PileType.Deck (acquisition) order, so pre-Planned cards keep their position in the deck, take the
// lowest slots, and always sort before any Planned applied later in the combat.
public static class PrePlannedSetup
{
    // Per-player, per-combat guard: AfterPlayerTurnStartLate fires once per card (and per player in
    // multiplayer), but the ordered pass must run exactly once for each player each combat.
    private static readonly ConditionalWeakTable<Player, object> _assignedFor = new();

    // A card is pre-Planned if its class declares it (Signature, upgraded Experience) or it carries a
    // persistent PrePlanned enchantment applied out of combat by the Score relic.
    public static bool IsPrePlanned(CardModel card) =>
        (card is UnderstudyCard uc && uc.IsPrePlanned) || card.Enchantment is PrePlanned;

    // Pure: order pre-Planned cards by deck-acquisition rank, stable for equal ranks (identical
    // duplicate copies keep their encounter order). The caller then hands each the next sequencer
    // slot in this order, so slots are unique, non-overlapping, and follow deck position.
    public static IReadOnlyList<T> OrderByDeckRank<T>(IReadOnlyList<T> prePlanned, Func<T, int> deckRank) =>
        prePlanned.OrderBy(deckRank).ToList();

    public static void AssignIfNeeded(Player player, ICombatState combat)
    {
        if (_assignedFor.TryGetValue(player, out var token) && ReferenceEquals(token, combat)) return;
        _assignedFor.AddOrUpdate(player, combat);

        var deck = PileType.Deck.GetPile(player).Cards.ToList();

        // Rank a combat clone by the position of its matching pre-Planned deck card (matched on id +
        // upgrade level, so an upgraded pre-Planned copy isn't confused with an un-upgraded sibling).
        int DeckRank(CardModel clone)
        {
            int idx = deck.FindIndex(d => IsPrePlanned(d)
                && d.Id.Equals(clone.Id) && d.CurrentUpgradeLevel == clone.CurrentUpgradeLevel);
            return idx < 0 ? int.MaxValue : idx;
        }

        var prePlanned = PlannedModifier.RelevantCards(player)
            .Where(c => IsPrePlanned(c) && !c.TryGetModifier<PlannedModifier>(out _))
            .ToList();

        foreach (var clone in OrderByDeckRank(prePlanned, DeckRank))
            PlannedModifier.ApplyPrePlanned(clone, combat);

        // Pre-Tuned enchantment (Foldable Stage): grant Tuned to any enchanted card not already Tuned.
        // Order-independent, so no coordination needed — just apply once. Driven here (not from the
        // enchantment's own hooks) so it works for any card type, including colorless cards.
        foreach (var card in PlannedModifier.RelevantCards(player).ToList())
            if (card.Enchantment is PreTuned && !card.TryGetModifier<TunedModifier>(out _))
                TunedModifier.Apply(card, combat, player.Piles.SelectMany(p => p.Cards));
    }
}
