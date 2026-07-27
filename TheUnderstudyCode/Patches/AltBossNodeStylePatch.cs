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
    // Flank bosses render at half size, laid out symmetrically about the default boss with this gap
    // between adjacent art edges (map units) so there is a small space and no overlap.
    private const float FlankScale = 0.75f;
    // When the act has a double boss the game shrinks the default boss to 0.75, and each flank also
    // carries a stacked second boss, so the flanks themselves render smaller to stay clear.
    private const float FlankScaleDoubleBoss = 0.5f;
    private const float FlankGap = 40f;

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

        // Double boss: the flank stacks a second boss and the default is shrunk to 0.75, so flanks render
        // smaller. Boss nodes scale about their centre, so the default's visual half-width/bottom depend
        // on its live scale — use it so the flanks align to the real default art, not its unscaled box.
        bool doubleBoss = alts.Any(a => a.IsSecond);
        float flankScale = doubleBoss ? FlankScaleDoubleBoss : FlankScale;
        float defaultScale = defaultBoss.Scale.Y;
        float defaultCentreX = defaultBoss.Position.X + defaultBoss.Size.X * 0.5f;
        float defaultBottomY = defaultBoss.Position.Y + defaultBoss.Size.Y * 0.5f
                               + defaultBoss.Size.Y * defaultScale * 0.5f;

        // Two passes: flanks first (positioned relative to the default boss), then chained second bosses
        // (positioned relative to their now-placed flank). A second boss reads its parent flank's node.
        int styled = 0;
        foreach (var alt in alts.Where(a => !a.IsSecond))
        {
            var bossNode = SwapToBossNode(__instance, runState, dict, points, alt);
            if (bossNode == null) continue;

            // Lay the two flanks out symmetrically about the default boss (its art is their midpoint),
            // scaled down, art BOTTOMS on the default art's bottom line, with a small gap. Scale about the
            // node centre (centre pivot) so the node centre (Position + Size/2) stays fixed.
            var half = bossNode.Size * 0.5f;
            float sep = defaultBoss.Size.X * defaultScale * 0.5f + FlankGap + defaultBoss.Size.X * flankScale * 0.5f;
            int dir = alt.Side == FlankSide.Left ? -1 : 1;
            float centreX = defaultCentreX + dir * sep;

            bossNode.PivotOffset = half;
            bossNode.Scale = new Vector2(flankScale, flankScale);
            bossNode.Position = new Vector2(centreX - half.X,
                defaultBottomY - half.Y - bossNode.Size.Y * flankScale * 0.5f);

            RedrawEdge(__instance, alt.ParentCoord, alt.Point.coord);
            styled++;
        }

        foreach (var alt in alts.Where(a => a.IsSecond))
        {
            if (!dict.TryGetValue(alt.ParentCoord, out var flankNode)) continue; // parent flank must exist
            var bossNode = SwapToBossNode(__instance, runState, dict, points, alt);
            if (bossNode == null) continue;

            // Stack the second boss directly above its flank: same art centre X, its art bottom sitting a
            // small gap above the flank art's top. The flank uses centre-pivot scaling too, so its visual
            // centre is flankNode.Position + Size/2 and its art top is that centre minus half the scaled art.
            var half = bossNode.Size * 0.5f;
            var flankCentre = flankNode.Position + flankNode.Size * 0.5f;
            float flankArtHalf = flankNode.Size.Y * flankScale * 0.5f;
            float secondArtHalf = bossNode.Size.Y * flankScale * 0.5f;
            float secondCentreY = (flankCentre.Y - flankArtHalf) - FlankGap - secondArtHalf;

            bossNode.PivotOffset = half;
            bossNode.Scale = new Vector2(flankScale, flankScale);
            bossNode.Position = new Vector2(flankCentre.X - half.X, secondCentreY - half.Y);

            RedrawEdge(__instance, alt.ParentCoord, alt.Point.coord);
            styled++;
        }

        // The old normal nodes' travelable state + visuals were computed during SetMap; recompute so the
        // swapped boss nodes get the right state (our AltBossTravelabilityPatch marks them travelable).
        Traverse.Create(__instance).Method("RecalculateTravelability").GetValue();
        __instance.RefreshAllPointVisuals();
        Log.Info($"[BookOfEndings] styled {styled} alt boss node(s) as full-art bosses");
    }

    // Replace an injected alt boss's plain NNormalMapPoint with a real NBossMapPoint (art loaded), added
    // to the tree so its _Ready builds the assigned boss's art. Returns the new node (caller positions it),
    // or null if the encounter can't be resolved.
    private static NBossMapPoint? SwapToBossNode(
        NMapScreen screen, IRunState runState,
        Dictionary<MapCoord, NMapPoint> dict, Control points, AltBossNode alt)
    {
        var coord = alt.Point.coord;
        if (!dict.TryGetValue(coord, out var old)) return null;

        var enc = runState.Act.AllBossEncounters.FirstOrDefault(e => e.Id.ToString() == alt.EncounterId);
        if (enc == null)
        {
            Log.Warn($"[BookOfEndings] cannot style alt boss ({coord.col},{coord.row}): encounter " +
                     $"{alt.EncounterId} not in pool");
            return null;
        }

        // The game only preloads the default/second boss's node art; force-load this encounter's art so
        // _Ready resolves it instead of drawing blank.
        AltBossArtPreload.Ensure(enc);

        points.RemoveChild(old);
        old.QueueFree();
        dict.Remove(coord);

        var bossNode = NBossMapPoint.Create(alt.Point, screen, runState);
        // Add before positioning: AddChild triggers _Ready (art) plus a one-time anchor shift, so we set
        // the final position afterwards.
        points.AddChildSafely(bossNode);
        dict[coord] = bossNode;
        return bossNode;
    }

    // Remove and re-draw the parent->child path segments so the line meets the moved boss node. Uses the
    // screen's own private GetLineEndpoint/CreatePath (which handle the boss node's centre offset) and its
    // _paths / _pathsContainer stores, all via Traverse.
    private static void RedrawEdge(NMapScreen screen, MapCoord parent, MapCoord child)
    {
        var t = Traverse.Create(screen);
        var paths = t.Field("_paths")
            .GetValue<Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>>();
        var container = t.Field("_pathsContainer").GetValue<Node>();
        var dict = t.Field("_mapPointDictionary").GetValue<Dictionary<MapCoord, NMapPoint>>();
        if (paths == null || dict == null) return;

        var key = (parent, child);
        if (paths.TryGetValue(key, out var oldSegs))
        {
            foreach (var seg in oldSegs)
            {
                container?.RemoveChild(seg);
                seg.QueueFree();
            }
            paths.Remove(key);
        }

        if (!dict.TryGetValue(parent, out var pNode) || !dict.TryGetValue(child, out var cNode)) return;
        var from = t.Method("GetLineEndpoint", pNode).GetValue<Vector2>();
        var to = t.Method("GetLineEndpoint", cNode).GetValue<Vector2>();
        var segs = t.Method("CreatePath", from, to).GetValue<IReadOnlyList<TextureRect>>();
        if (segs != null) paths[key] = segs;
    }
}
