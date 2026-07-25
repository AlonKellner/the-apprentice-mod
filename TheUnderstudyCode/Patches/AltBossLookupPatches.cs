using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Coordinate lookups must also find the injected alt bosses, or travel/rendering can't resolve them
// from their coord. GetPoint(int,int) and HasPoint(MapCoord) are the two lookups the engine uses; the
// base special-cases only Boss/SecondBoss, so we fall through to AltBossStore when the base misses.

[HarmonyPatch(typeof(ActMap), nameof(ActMap.GetPoint), new[] { typeof(int), typeof(int) })]
public static class AltBossGetPointPatch
{
    [HarmonyPostfix]
    public static void Postfix(ActMap __instance, int col, int row, ref MapPoint? __result)
    {
        if (__result != null) return;
        __result = AltBossStore.For(__instance).FirstOrDefault(p => p.coord.col == col && p.coord.row == row);
    }
}

[HarmonyPatch(typeof(ActMap), nameof(ActMap.HasPoint), new[] { typeof(MapCoord) })]
public static class AltBossHasPointPatch
{
    [HarmonyPostfix]
    public static void Postfix(ActMap __instance, MapCoord coord, ref bool __result)
    {
        if (!__result)
            __result = AltBossStore.For(__instance).Any(p => p.coord.col == coord.col && p.coord.row == coord.row);
    }
}
