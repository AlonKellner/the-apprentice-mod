using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// SetMap creates a plain NNormalMapPoint for every GetAllMapPoints entry, and a normal node renders no
// icon for a Boss point (MapPointType.Boss => empty) — so our injected alt bosses come out blank. The
// game already renders a second, distinct full-art boss on the same map (SecondBossMapPoint -> its own
// NBossMapPoint), so we follow that same approach: after SetMap, swap each alt boss's normal node for a
// real NBossMapPoint at the same position, then re-point its art at the boss that flank leads to. Finally
// recompute travelability/visuals so the swapped nodes light up correctly.
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
public static class AltBossNodeStylePatch
{
    [HarmonyPostfix]
    public static void Postfix(NMapScreen __instance, ActMap map)
    {
        var alts = AltBossStore.For(map);
        if (alts.Count == 0) return;

        var t = Traverse.Create(__instance);
        var runState = t.Field("_runState").GetValue<IRunState>();
        var dict = t.Field("_mapPointDictionary").GetValue<Dictionary<MapCoord, NMapPoint>>();
        var points = t.Field("_points").GetValue<Control>();
        var defaultBoss = t.Field("_bossPointNode").GetValue<NMapPoint>();
        if (runState == null || dict == null || points == null || defaultBoss == null) return;

        int styled = 0;
        foreach (var alt in alts)
        {
            var coord = alt.Point.coord;
            if (!dict.TryGetValue(coord, out var old)) continue;

            var enc = runState.Act.AllBossEncounters.FirstOrDefault(e => e.Id.ToString() == alt.EncounterId);
            if (enc == null)
            {
                Log.Warn($"[BookOfOrder] cannot style alt boss ({coord.col},{coord.row}): encounter " +
                         $"{alt.EncounterId} not in pool");
                continue;
            }

            // The game only preloads the default/second boss's node art (ActModel.MapNodeAssetPaths),
            // never the flanks'. Until that art is loaded, the node renders blank. Force-load exactly the
            // paths the node will request (spine .tres, or the placeholder PNGs) so _Ready resolves them.
            AltBossArtPreload.Ensure(enc);

            float x = dict.TryGetValue(alt.ParentRestCoord, out var rest) ? rest.Position.X : old.Position.X;

            points.RemoveChild(old);
            old.QueueFree();
            dict.Remove(coord);

            var bossNode = NBossMapPoint.Create(alt.Point, __instance, runState);
            // AddChild triggers _Ready (which builds this flank's own boss art in one clean pass) AND a
            // one-time anchor shift on the boss node. Position AFTER the add so we align to the default
            // boss's already-shifted coordinates instead of getting shifted a second time: same Y as the
            // default boss, at this flank's own rest X, so the three bosses sit left / centre / right.
            points.AddChildSafely(bossNode);
            bossNode.Position = new Vector2(x, defaultBoss.Position.Y);
            bossNode.Scale = defaultBoss.Scale;
            dict[coord] = bossNode;
            styled++;
            Log.Info($"[BookOfOrder] placed flank {alt.Side}: set ({x},{defaultBoss.Position.Y}) " +
                     $"reads back {bossNode.Position}; default boss at {defaultBoss.Position}");
        }

        // The old normal nodes' travelable state + visuals were computed during SetMap; recompute so the
        // swapped boss nodes get the right state (our AltBossTravelabilityPatch marks them travelable).
        Traverse.Create(__instance).Method("RecalculateTravelability").GetValue();
        __instance.RefreshAllPointVisuals();
        Log.Info($"[BookOfOrder] styled {styled} alt boss node(s) as full-art bosses");
    }
}
