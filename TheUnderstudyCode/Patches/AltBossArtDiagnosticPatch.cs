using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// TEMP DIAGNOSTIC: the transpiler + node swap both fire (log confirms "renders art for ..."), yet the
// flank art is invisible. This postfix dumps every boss node's visual state right after _Ready so we can
// compare a working default boss against a blank flank: whether it resolved to spine or placeholder, the
// node's position/scale/visibility, and the spine sprite's own global position (in case the art renders
// off where the clickable node sits). Remove once the cause is found.
[HarmonyPatch(typeof(NBossMapPoint), "_Ready")]
public static class AltBossArtDiagnosticPatch
{
    [HarmonyPostfix]
    public static void Postfix(NBossMapPoint __instance)
    {
        var t = Traverse.Create(__instance);
        var usesSpine = t.Field("_usesSpine").GetValue<bool>();
        var coord = __instance.Point.coord;

        string placeholder = "n/a";
        if (!usesSpine)
        {
            var img = t.Field("_placeholderImage").GetValue<Godot.TextureRect>();
            var outline = t.Field("_placeholderOutline").GetValue<Godot.TextureRect>();
            placeholder = img == null
                ? "img=null"
                : $"imgVisible={img.Visible} imgTex={(img.Texture != null ? img.Texture.ResourcePath : "null")} " +
                  $"imgGlobalPos={img.GlobalPosition} imgSize={img.Size} imgModulate={img.SelfModulate} " +
                  $"outlineTex={(outline?.Texture != null ? "set" : "null")}";
        }

        Log.Info($"[BossArtDiag] node ({coord.col},{coord.row}) type={__instance.Point.PointType} " +
                 $"usesSpine={usesSpine} nodeVisible={__instance.Visible} pos={__instance.Position} " +
                 $"scale={__instance.Scale} placeholder[{placeholder}]");
    }
}
