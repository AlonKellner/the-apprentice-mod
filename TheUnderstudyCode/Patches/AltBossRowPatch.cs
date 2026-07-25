using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// GetPointsInRow only walks the grid, so the boss row (which lives outside the grid) normally comes back
// empty — which means free travel (Winged Boots picks any node in the next row) and any row-based layout
// can't see the injected alt bosses. Append the store's alt bosses whose row matches the requested row.
// Normal edge-following travel doesn't need this (the alt bosses are real child points of their rest),
// but free travel and completeness do.
[HarmonyPatch(typeof(ActMap), nameof(ActMap.GetPointsInRow))]
public static class AltBossGetPointsInRowPatch
{
    [HarmonyPostfix]
    public static void Postfix(ActMap __instance, int row, ref IEnumerable<MapPoint> __result)
    {
        var extra = AltBossStore.For(__instance).Where(n => n.Point.coord.row == row).Select(n => n.Point).ToList();
        if (extra.Count > 0) __result = __result.Concat(extra);
    }
}
