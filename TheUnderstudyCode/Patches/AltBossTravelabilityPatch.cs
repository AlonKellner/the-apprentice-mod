using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// NMapScreen.RecalculateTravelability special-cases the pre-boss row: when the last visited coord is on
// it, the method sets ONLY the default boss node travelable and returns early, never walking the current
// point's child edges. Our alt bosses are real child edges of the flank rests, so that shortcut leaves
// them Untravelable — you can't click them in normal play (debug `travel` bypasses this, which is why it
// looked fine). This postfix runs after the base method and marks travelable any alt boss reachable from
// the current point, using the game's own MapTravel.GetTravelablePointsFrom (which returns the current
// point's Children, or the next row under free travel / Winged Boots — both already include the alts).
[HarmonyPatch(typeof(NMapScreen), "RecalculateTravelability")]
public static class AltBossTravelabilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(NMapScreen __instance)
    {
        var t = Traverse.Create(__instance);
        var runState = t.Field("_runState").GetValue<RunState>();
        var map = t.Field("_map").GetValue<ActMap>();
        var dict = t.Field("_mapPointDictionary").GetValue<Dictionary<MapCoord, NMapPoint>>();
        if (runState == null || map == null || dict == null) return;
        if (AltBossStore.For(map).Count == 0) return;

        var visited = runState.VisitedMapCoords;
        if (visited.Count == 0) return;
        var currentPoint = map.GetPoint(visited[visited.Count - 1]);
        if (currentPoint == null) return;

        foreach (var reachable in MapTravel.GetTravelablePointsFrom(runState, currentPoint))
        {
            if (AltBossStore.IsAltBoss(map, reachable.coord) && dict.TryGetValue(reachable.coord, out var node))
                node.State = MapPointState.Travelable;
        }
    }
}
