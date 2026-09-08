using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Map;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// One injected alternative boss: the live MapPoint, which flank it belongs to, the coord of the node it
// hangs from (the pre-boss rest for a flank, or the flank boss for its chained second boss), the
// encounter id the player fights there, and whether it is a chained second boss (Ascension-10 double
// boss). The encounter is stored here (not on the MapPoint, which has no slot for it) so
// AltBossEncounterPatch can substitute the right boss per node at room-creation time; the parent coord
// lets the styling patch place the node relative to what it chains from (boss nodes are hand-placed
// above the grid, not by the grid formula).
public sealed record AltBossNode(
    MapPoint Point, FlankSide Side, string EncounterId, MapCoord ParentCoord, bool IsSecond);

// Per-map registry of the alternative boss nodes the Book of Endings injects. They live outside the base
// ActMap's fixed Boss/SecondBoss slots, so the enumeration/lookup patches (AltBoss*Patch) read them
// from here to make the rest of the engine — rendering (SetMap iterates GetAllMapPoints), travel, and
// save-enumeration — see them. Keyed by the live ActMap via ConditionalWeakTable so entries are
// collected with the map and never leak across acts. The map has no slot for these, so on load they
// are re-injected deterministically (AltBossPlan) rather than serialized.
public static class AltBossStore
{
    private static readonly ConditionalWeakTable<ActMap, List<AltBossNode>> ByMap = new();

    public static void Register(ActMap map, AltBossNode node) => ByMap.GetOrCreateValue(map).Add(node);

    public static IReadOnlyList<AltBossNode> For(ActMap map) =>
        ByMap.TryGetValue(map, out var list) ? list : (IReadOnlyList<AltBossNode>)Array.Empty<AltBossNode>();

    // The encounter id assigned to the alt boss at this coord, or null if the coord is not an alt boss.
    public static string? EncounterAt(ActMap map, MapCoord coord)
    {
        foreach (var n in For(map))
            if (n.Point.coord.col == coord.col && n.Point.coord.row == coord.row) return n.EncounterId;
        return null;
    }

    // Whether a coordinate is one of this map's injected alt bosses.
    public static bool IsAltBoss(ActMap map, MapCoord coord) => EncounterAt(map, coord) != null;

    // The injected alt boss node at this coord, or null.
    public static AltBossNode? NodeAt(ActMap map, MapCoord coord)
    {
        foreach (var n in For(map))
            if (n.Point.coord.col == coord.col && n.Point.coord.row == coord.row) return n;
        return null;
    }

    // True when `coord` is an alt FIRST boss (IsSecond == false) that has a chained alt SECOND boss still
    // to fight — the Ascension-10 double-boss case the base reward screen misses. The base game only opens
    // the map to travel to the second boss when you're on the DEFAULT first boss (Map.BossMapPoint.coord),
    // so an alt first boss otherwise jumps straight to the act ending; AltBossSecondBossProceedPatch uses
    // this to plug that gap. The chained second registers with ParentCoord == its first boss's coord.
    public static bool HasChainedSecondBoss(ActMap map, MapCoord coord)
    {
        var node = NodeAt(map, coord);
        if (node == null || node.IsSecond) return false;
        foreach (var n in For(map))
            if (n.IsSecond && n.ParentCoord.col == coord.col && n.ParentCoord.row == coord.row) return true;
        return false;
    }
}
