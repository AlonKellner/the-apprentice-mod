using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The base ActMap only knows its fixed Boss/SecondBoss points, so the alt bosses the Book of Endings
// injects (AltBossStore) are invisible to it. SetMap renders the map by iterating GetAllMapPoints and
// creating a node per point, so appending the store's alt bosses here is what makes them render and be
// drawn/travelled. Top-level class-level [HarmonyPatch] so PatchAll discovers it (see the mod's other
// patches). Applies to every ActMap subclass (Standard/Saved), which is what we want.
[HarmonyPatch(typeof(ActMap), nameof(ActMap.GetAllMapPoints))]
public static class AltBossGetAllMapPointsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ActMap __instance, ref IEnumerable<MapPoint> __result)
    {
        var extra = AltBossStore.For(__instance);
        if (extra.Count > 0) __result = __result.Concat(extra.Select(n => n.Point));
    }
}
