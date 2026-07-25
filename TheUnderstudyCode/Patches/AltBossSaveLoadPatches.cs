using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Saves.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The base save format has no slot for injected alt bosses, and SavedActMap places every serialized
// point into a fixed grid by coord — so an alt boss (row = boss row, outside the grid) crashes the load
// with IndexOutOfRange. The alt bosses must never reach the save; they are re-injected deterministically
// on load instead (BookOfOrder.ModifyGeneratedMapLate). These two patches keep the save clean and
// tolerate saves written before this fix.

// On save: FromActMap serializes GetAllMapPoints (which our postfix augments with alt bosses), so strip
// the alt-boss points and any edges pointing at them back out of the serialized result.
[HarmonyPatch(typeof(SerializableActMap), nameof(SerializableActMap.FromActMap))]
public static class AltBossStripOnSavePatch
{
    [HarmonyPostfix]
    public static void Postfix(ActMap map, ref SerializableActMap __result)
    {
        var alt = AltBossStore.For(map);
        if (alt.Count == 0) return;

        var altCoords = new HashSet<MapCoord>(alt.Select(p => p.coord));
        __result.Points.RemoveAll(p => altCoords.Contains(p.Coord));
        foreach (var p in __result.Points)
            p.ChildCoords?.RemoveAll(c => altCoords.Contains(c));
    }
}

// On load: defensively drop any point whose coord falls outside the grid (e.g. an alt boss saved before
// the strip-on-save patch existed), plus edges to it, so an old save loads instead of IndexOutOfRange.
// The alt boss is then re-injected by ModifyGeneratedMapLate.
[HarmonyPatch(typeof(SavedActMap), MethodType.Constructor, new[] { typeof(SerializableActMap) })]
public static class AltBossTolerateOldSavePatch
{
    [HarmonyPrefix]
    public static void Prefix(SerializableActMap saved)
    {
        int w = saved.GridWidth, h = saved.GridHeight;
        var outOfGrid = new HashSet<MapCoord>(saved.Points
            .Where(p => p.Coord.col < 0 || p.Coord.col >= w || p.Coord.row < 0 || p.Coord.row >= h)
            .Select(p => p.Coord));
        if (outOfGrid.Count == 0) return;

        saved.Points.RemoveAll(p => outOfGrid.Contains(p.Coord));
        foreach (var p in saved.Points)
            p.ChildCoords?.RemoveAll(c => outOfGrid.Contains(c));
    }
}
