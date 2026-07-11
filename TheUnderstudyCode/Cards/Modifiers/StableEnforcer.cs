using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// The frozen configuration of a Stable card: each BaseLib modifier captured as its own INSTANCE plus a
// snapshot of its internal state, and the card's local keywords (Ethereal/Retain/etc.).
//
// We keep the modifier instance itself — not a Type or a rebuilt copy — because a modifier is a ModelDb
// model: `new`-ing one (Activator or constructor) in a live combat throws DuplicateModelException. The
// original instance is a valid mutable clone, so restore re-attaches that same object and only resets its
// state via LoadSaveData, never constructing anything.
public sealed class StableState
{
    public IReadOnlyList<(CardModifier modifier, CardModifier.ModifierSave save)> Modifiers { get; }
    public IReadOnlySet<CardKeyword> LocalKeywords { get; }

    public StableState(
        IReadOnlyList<(CardModifier modifier, CardModifier.ModifierSave save)> modifiers,
        IReadOnlySet<CardKeyword> localKeywords)
    {
        Modifiers = modifiers;
        LocalKeywords = localKeywords;
    }
}

// Engine-independent core of the Stable "freeze": snapshot a card's modifiers + local keywords, then
// reconcile the live card back to that snapshot — undoing in-place mutation (e.g. an emptied Planned slot
// list), foreign modifier additions, and foreign keyword add/removes from ANY source. Kept free of
// ModelDb construction / CombatState / Godot / Log so it runs against bare-constructed cards in unit
// tests; the live hook wiring and RefreshVisualIndices (which need a live combat / card Owner) stay in
// UnderstudyCard.
public static class StableEnforcer
{
    // Set while Restore is applying, so the ApplyInternal Harmony patch (which fires on each keyword-driven
    // event) doesn't recursively re-enter enforcement.
    [ThreadStatic] public static bool Enforcing;

    public static StableState Capture(CardModel card)
    {
        var mods = CardModifier.DirectModifiers(card).Select(m => (m, SaveOf(m))).ToList();
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
            var frozen = new HashSet<CardModifier>(snap.Modifiers.Select(x => x.modifier));

            // 1. Strip foreign modifiers — an instance not part of the frozen config.
            foreach (var m in live.Where(m => !frozen.Contains(m)).ToList())
                live.Remove(m);

            // 2. Re-attach any detached frozen instance (the SAME object — no construction), and reset
            //    every frozen instance's internal state (fixes in-place mutation like an emptied Planned
            //    slot list).
            foreach (var (modifier, save) in snap.Modifiers)
            {
                if (!live.Contains(modifier))
                    live.Add(modifier);
                modifier.LoadSaveData(save);
            }

            // 3. Reconcile local keywords (application needs a live mutable card; the decision is pure).
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
        var live = CardModifier.DirectModifiers(card);
        if (live.Count != snap.Modifiers.Count) return false;

        var frozen = snap.Modifiers.ToDictionary(x => x.modifier, x => x.save);
        foreach (var m in live)
        {
            // Same set of instances (reference identity) …
            if (!frozen.TryGetValue(m, out var save)) return false;
            // … and no in-place state drift on any of them.
            if (!SavesEqual(SaveOf(m), save)) return false;
        }

        var localNow = card.GetKeywordsWithSources(KeywordSources.Local);
        return localNow.Count == snap.LocalKeywords.Count && localNow.All(snap.LocalKeywords.Contains);
    }

    private static CardModifier.ModifierSave SaveOf(CardModifier m)
    {
        // Build the save manually rather than via ModifierSave.FromModifier: FromModifier reads
        // modifier.Id, which is unset on the bare instances used in tests. Reconciliation keys by instance
        // identity, so Id is irrelevant here anyway.
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
