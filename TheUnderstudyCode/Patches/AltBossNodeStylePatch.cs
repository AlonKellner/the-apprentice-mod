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
    private const float FlankScale = 0.5f;
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

        Log.Info($"[BookOfOrder] default boss: pos={defaultBoss.Position} size={defaultBoss.Size} " +
                 $"scale={defaultBoss.Scale} artGlobal={ArtGlobalPos(defaultBoss)} " +
                 $"pointsRect={points.GetGlobalRect()}");

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

            points.RemoveChild(old);
            old.QueueFree();
            dict.Remove(coord);

            var bossNode = NBossMapPoint.Create(alt.Point, __instance, runState);
            // AddChild triggers _Ready (which builds this flank's own boss art in one clean pass) AND a
            // one-time anchor shift on the boss node. Position AFTER the add so we align to the default
            // boss's already-shifted coordinates instead of getting shifted a second time.
            points.AddChildSafely(bossNode);

            // Lay the two flanks out symmetrically about the default boss so it is their midpoint, at the
            // same art height, half-size, with a small gap and no overlap. A boss node's art centres on
            // the node's centre (Position + Size/2), so: half-size via Scale (centre pivot so scaling
            // keeps that centre fixed), then place the node so its centre lands `sep` left/right of the
            // default boss's centre. sep = half the default art + gap + half the (scaled) flank art.
            var size = bossNode.Size;
            var defaultCentre = defaultBoss.Position + defaultBoss.Size * 0.5f;
            float sep = defaultBoss.Size.X * 0.5f + FlankGap + defaultBoss.Size.X * FlankScale * 0.5f;
            int dir = alt.Side == FlankSide.Left ? -1 : 1;
            var artCentre = new Vector2(defaultCentre.X + dir * sep, defaultCentre.Y);

            bossNode.PivotOffset = size * 0.5f;               // scale about the centre, not the corner
            bossNode.Scale = new Vector2(FlankScale, FlankScale);
            bossNode.Position = artCentre - size * 0.5f;      // node centre == artCentre
            dict[coord] = bossNode;

            // The incoming path was drawn during SetMap while this was an NNormalMapPoint (endpoint =
            // Position, the old grid cell); redraw the rest->flank edge so it meets the boss node's centre
            // (GetLineEndpoint = Position + Size/2 = artCentre) instead of the old cell.
            RedrawEdge(__instance, alt.ParentRestCoord, coord);
            styled++;
            Log.Info($"[BookOfOrder] placed flank {alt.Side}: sep={sep} artCentre={artCentre} " +
                     $"nodePos={bossNode.Position} nodeSize={size} scale={FlankScale} artGlobal={ArtGlobalPos(bossNode)}");
        }

        // The old normal nodes' travelable state + visuals were computed during SetMap; recompute so the
        // swapped boss nodes get the right state (our AltBossTravelabilityPatch marks them travelable).
        Traverse.Create(__instance).Method("RecalculateTravelability").GetValue();
        __instance.RefreshAllPointVisuals();
        Log.Info($"[BookOfOrder] styled {styled} alt boss node(s) as full-art bosses");
    }

    // DIAGNOSTIC: where a boss node's art actually renders (spine sprite or placeholder image), so we can
    // compare a flank's art position against the default boss and the map bounds.
    private static Vector2 ArtGlobalPos(NMapPoint node)
    {
        var t = Traverse.Create(node);
        if (t.Field("_usesSpine").GetValue<bool>())
        {
            var s = t.Field("_spineSprite").GetValue<Node2D>();
            return s != null ? s.GlobalPosition : new Vector2(float.NaN, float.NaN);
        }
        var img = t.Field("_placeholderImage").GetValue<TextureRect>();
        return img != null ? img.GlobalPosition : new Vector2(float.NaN, float.NaN);
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
