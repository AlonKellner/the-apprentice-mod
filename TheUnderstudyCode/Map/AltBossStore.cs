using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Map;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// Per-map registry of the alternative boss nodes the Book of Order injects. They live outside the base
// ActMap's fixed Boss/SecondBoss slots, so the enumeration/lookup patches (AltBossMapPatches) read
// them from here to make the rest of the engine — rendering (SetMap iterates GetAllMapPoints), travel,
// and save-enumeration — see them. Keyed by the live ActMap via ConditionalWeakTable so entries are
// collected with the map and never leak across acts. The map has no slot for these, so on load they
// are re-injected deterministically rather than serialized (Phase 3 / Spike C).
public static class AltBossStore
{
    private static readonly ConditionalWeakTable<ActMap, List<MapPoint>> ByMap = new();

    public static void Register(ActMap map, MapPoint altBoss) => ByMap.GetOrCreateValue(map).Add(altBoss);

    public static IReadOnlyList<MapPoint> For(ActMap map) =>
        ByMap.TryGetValue(map, out var list) ? list : Array.Empty<MapPoint>();

    // Whether a coordinate is one of this map's injected alt bosses — used at boss-room creation to
    // decide whether to substitute the alt encounter.
    public static bool IsAltBoss(ActMap map, MapCoord coord)
    {
        foreach (var p in For(map))
            if (p.coord.col == coord.col && p.coord.row == coord.row) return true;
        return false;
    }
}
