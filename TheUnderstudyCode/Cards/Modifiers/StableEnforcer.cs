using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// The frozen configuration of a Stable card: each BaseLib modifier captured with its internal state
// (keyed by runtime Type — ModelId is unset on bare/reconstructed instances), plus the card's local
// keywords (Ethereal/Retain/etc. added via CardModel.AddKeyword).
public sealed class StableState
{
    public IReadOnlyList<(Type type, int amount, CardModifier.ModifierSave save)> Modifiers { get; }
    public IReadOnlySet<CardKeyword> LocalKeywords { get; }

    public StableState(
        IReadOnlyList<(Type type, int amount, CardModifier.ModifierSave save)> modifiers,
        IReadOnlySet<CardKeyword> localKeywords)
    {
        Modifiers = modifiers;
        LocalKeywords = localKeywords;
    }
}

// Engine-independent core of the Stable "freeze": snapshot a card's modifiers + local keywords, then
// reconcile the live card back to that snapshot — undoing in-place mutation (e.g. an emptied Planned
// slot list), foreign modifier additions, and foreign keyword add/removes from ANY source. Kept free of
// ModelDb / CombatState / Godot / Log so it runs against bare-constructed cards in unit tests; the live
// hook wiring and RefreshVisualIndices (which need a live combat / card Owner) stay in UnderstudyCard.
public static class StableEnforcer
{
    // Set while Restore is applying its own re-adds, so the ApplyInternal Harmony patch (which fires on
    // each re-add) doesn't recursively re-enter enforcement.
    [ThreadStatic] public static bool Enforcing;

    public static StableState Capture(CardModel card)
    {
        var mods = CardModifier.DirectModifiers(card)
            .Select(m => (m.GetType(), m.Amount, SaveOf(m)))
            .ToList();
        var keywords = new HashSet<CardKeyword>(card.GetKeywordsWithSources(KeywordSources.Local));
        return new StableState(mods, keywords);
    }

    // Reconciles `card` back to `snap`. Returns whether anything was changed. No-op (returns false) when
    // already reentrant or already matching.
    public static bool Restore(CardModel card, StableState snap)
    {
        if (Enforcing) return false;
        if (Matches(card, snap)) return false;

        Enforcing = true;
        try
        {
            var live = CardModifier.DirectModifiers(card);
            var snapTypes = snap.Modifiers.Select(x => x.type).ToHashSet();

            // 1. Strip foreign modifiers — a type not present in the frozen snapshot.
            foreach (var m in live.Where(m => !snapTypes.Contains(m.GetType())).ToList())
                live.Remove(m);

            // 2. Reset present modifiers in place (fixes in-place mutation like an emptied Planned slot
            //    list) / rebuild any that were fully detached, restoring internal state either way.
            foreach (var (type, _, save) in snap.Modifiers)
            {
                // Note: we don't reassign CardModifier.Amount — its setter AssertMutable-throws on a
                // canonical instance, and none of this mod's modifiers carry state in Amount (Planned →
                // slots, Tense → Stacks, etc., all round-tripped through Store/LoadSaveData). The saved
                // Amount is kept in the snapshot for completeness / future Amount-based modifiers.
                var existing = live.FirstOrDefault(m => m.GetType() == type);
                if (existing != null)
                {
                    existing.LoadSaveData(save);
                }
                else
                {
                    var rebuilt = (CardModifier)Activator.CreateInstance(type)!;
                    rebuilt.LoadSaveData(save);
                    CardModifier.AddModifier(card, rebuilt);
                }
            }

            // 3. Reconcile local keywords. AddKeyword/RemoveKeyword assert the card is mutable (only
            //    true in a live combat), so the diff is a pure, testable step and only the application
            //    touches the live card.
            var localNow = new HashSet<CardKeyword>(card.GetKeywordsWithSources(KeywordSources.Local));
            var (toAdd, toRemove) = KeywordDiff(snap.LocalKeywords, localNow);
            foreach (var kw in toAdd) card.AddKeyword(kw);
            foreach (var kw in toRemove) card.RemoveKeyword(kw);

            return true;
        }
        finally
        {
            Enforcing = false;
        }
    }

    // Pure keyword reconciliation: given the frozen local keyword set and the current one, which to add
    // back and which to strip. Separated from application (AddKeyword/RemoveKeyword need a live mutable
    // card) so the logic is unit-testable.
    public static (List<CardKeyword> toAdd, List<CardKeyword> toRemove) KeywordDiff(
        IReadOnlySet<CardKeyword> frozen, IReadOnlySet<CardKeyword> current)
    {
        var toAdd = frozen.Where(k => !current.Contains(k)).ToList();
        var toRemove = current.Where(k => !frozen.Contains(k)).ToList();
        return (toAdd, toRemove);
    }

    private static bool Matches(CardModel card, StableState snap)
    {
        var current = Capture(card);
        if (current.Modifiers.Count != snap.Modifiers.Count) return false;

        var bySnapType = snap.Modifiers.ToDictionary(x => x.type, x => (x.amount, x.save));
        foreach (var (type, amount, save) in current.Modifiers)
        {
            if (!bySnapType.TryGetValue(type, out var s)) return false;
            if (s.amount != amount || !SavesEqual(s.save, save)) return false;
        }

        return current.LocalKeywords.Count == snap.LocalKeywords.Count
            && current.LocalKeywords.All(snap.LocalKeywords.Contains);
    }

    private static CardModifier.ModifierSave SaveOf(CardModifier m)
    {
        // Build the save manually rather than via ModifierSave.FromModifier: FromModifier reads
        // modifier.Id, which is unset on the bare instances used in tests. We key by runtime Type, so Id
        // is irrelevant here anyway.
        var save = new CardModifier.ModifierSave { Amount = m.Amount };
        m.StoreSaveData(save);
        return save;
    }

    private static bool SavesEqual(CardModifier.ModifierSave a, CardModifier.ModifierSave b) =>
        a.Amount == b.Amount
        && DictEqual(a.IntProperties, b.IntProperties)
        && DictEqual(a.AdditionalProperties, b.AdditionalProperties);

    private static bool DictEqual<TK, TV>(Dictionary<TK, TV> a, Dictionary<TK, TV> b) where TK : notnull
    {
        if (a.Count != b.Count) return false;
        foreach (var kv in a)
            if (!b.TryGetValue(kv.Key, out var v) || !EqualityComparer<TV>.Default.Equals(v, kv.Value))
                return false;
        return true;
    }
}
