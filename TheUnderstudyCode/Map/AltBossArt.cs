using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// Re-points a freshly-created NBossMapPoint's art at the boss its flank actually leads to. NBossMapPoint
// resolves its art in _Ready from the act's default/second boss and ignores the point itself, so once the
// node is in the tree we overwrite the art with the assigned encounter — mirroring the exact spine /
// placeholder branches the game uses (see NBossMapPoint._Ready), so a flank shows the real animated boss
// it routes to. Reflection into the node's private fields via Traverse; this is display-only game code.
public static class AltBossArt
{
    public static void ApplyEncounterArt(NBossMapPoint node, EncounterModel enc)
    {
        var t = Traverse.Create(node);
        var act = t.Field("_act").GetValue<ActModel>();
        var spineSprite = t.Field("_spineSprite").GetValue<Node2D>();
        var animController = t.Field("_animController").GetValue<MegaSprite>();

        var spine = enc.BossNodeSpineResource;
        if (spine != null)
        {
            t.Field("_usesSpine").SetValue(true);
            if (spineSprite != null) spineSprite.Visible = true;
            animController.SetSkeletonDataRes(spine);
            animController.GetAnimationState().AddAnimation("animation");
            t.Field("_material").SetValue((ShaderMaterial)animController.GetNormalMaterial()!);
        }
        else
        {
            // Rare fallback: a boss with no spine resource uses two placeholder PNGs. Those TextureRects
            // are only fetched inside _Ready's else-branch, so grab them by node path if the field is null.
            t.Field("_usesSpine").SetValue(false);
            if (spineSprite != null) spineSprite.Visible = false;
            TextureRect? img = t.Field("_placeholderImage").GetValue<TextureRect>();
            TextureRect? outline = t.Field("_placeholderOutline").GetValue<TextureRect>();
            img ??= node.GetNode<TextureRect>("%PlaceholderImage");
            outline ??= node.GetNode<TextureRect>("%PlaceholderOutline");
            img.Visible = true;
            img.Texture = PreloadManager.Cache.GetAsset<Texture2D>(enc.BossNodePath + ".png");
            outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>(enc.BossNodePath + "_outline.png");
            img.SelfModulate = act.MapTraveledColor;
            outline.SelfModulate = act.MapBgColor;
        }

        t.Method("RefreshColorInstantly").GetValue();
    }
}
