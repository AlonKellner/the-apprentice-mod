using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

// Records, per creature, the order in which swappable powers were last modified — so Swap can act on
// each creature's *most recently modified* swappable debuff (give side) and buff (take side) rather
// than a fixed "each" sweep. Written only by SwapRecencyPatch (a postfix on the single global
// Hook.AfterPowerAmountChanged chokepoint every real power-amount change funnels through), read only
// by SceneStealing's selection.
//
// Keyed by Creature via a ConditionalWeakTable: a new combat's fresh Creature instances start with no
// history and last combat's entries are collected once those creatures are unreachable, so there is no
// per-combat reset to forget. The sequence counter is a single monotonic long — only its relative order
// within a combat matters, and long won't realistically overflow.
public static class SwapRecency
{
    private static readonly ConditionalWeakTable<Creature, Dictionary<string, long>> _byCreature = new();
    private static long _seq;

    public static void Record(Creature creature, string powerEntry)
    {
        var map = _byCreature.GetOrCreateValue(creature);
        map[powerEntry] = ++_seq;
    }

    // Higher = more recently modified. long.MinValue means this creature has no record of that power
    // (e.g. an innate buff applied before this observer ever saw it) — the caller then falls back to
    // registry order so Swap never silently no-ops on a present-but-untracked power.
    public static long LastModified(Creature creature, string powerEntry) =>
        _byCreature.TryGetValue(creature, out var map) && map.TryGetValue(powerEntry, out var seq)
            ? seq : long.MinValue;
}
