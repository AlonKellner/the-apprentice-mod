using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Attaches Order's red shimmer overlay directly in code instead of via
// AfflictionModel.OverlayPath/CreateOverlay() (see generate_order_overlay.py for why that
// scene+PreloadManager convention was abandoned) — loads the shader directly with GD.Load and
// assigns it to a plain ColorRect's Material, per
// https://github.com/Alchyr/ModTemplate-StS2/wiki/Shaders's "apply a shader to a node without
// children" pattern.
//
// Sizing/positioning: %OverlayContainer (a sibling of Frame/Shadow/Portrait under CardContainer)
// is NOT laid out via Godot's Control anchor system at all — logging confirmed every ancestor from
// NCard up through CardHolderContainer permanently reports Size=(0,0), even though the card renders
// normally. Every visible card element (Frame, Shadow, etc.) is instead manually centered on
// CardContainer's local origin via Position = -Size/2 (e.g. Frame: size=(300,422),
// position=(-150,-211)) — a hand-authored centered-rect convention, not FullRect anchoring. So our
// overlay copies Frame's exact Size/Position instead of anchoring against OverlayContainer (which
// would always resolve to a zero-size rect) or NCard.Size (also always (0,0)).
[HarmonyPatch(typeof(NCard), "ReloadOverlay")]
public static class OrderOverlayPatch
{
    // Lazy, NOT a static readonly field with an eager GD.Load initializer: that initializer would
    // run the instant Harmony patches this class during harmony.PatchAll() at mod bootstrap, which
    // can be before Godot's resource system has the mod's own PCK fully mounted — GD.Load can
    // silently return null at that point (no exception), leaving every ShaderMaterial built from it
    // shaderless and invisible with zero errors logged. Loading on first actual use (deep into an
    // active combat) sidesteps that ordering risk entirely.
    private static Shader? _overlayShader;
    private static Shader OverlayShader => _overlayShader ??= GD.Load<Shader>("res://shaders/order_overlay.gdshader");

    // A SINGLE shared ShaderMaterial for every card, not `new ShaderMaterial` per attach: the first
    // time the GPU ever renders a given shader/material pipeline, Godot has to compile it, which can
    // stall the whole frame (reported as the entire game stuttering, not just the card). Building a
    // fresh ShaderMaterial per card risked repeating that stall on every single card, every combat.
    // One shared instance means there's only ever one pipeline to compile all run, and Schedule()
    // below forces that compile to happen off-screen, well before a player ever needs to see it.
    private static ShaderMaterial? _overlayMaterial;
    private static ShaderMaterial OverlayMaterial => _overlayMaterial ??= new ShaderMaterial { Shader = OverlayShader };

    // NCard has no hook to store our own child alongside its private _cardOverlay field, so track
    // our attached overlay per-instance ourselves; ConditionalWeakTable avoids leaking if an NCard
    // is freed without ever going through the "affliction cleared" branch below.
    private static readonly ConditionalWeakTable<NCard, Control> AttachedOverlays = new();

    private static readonly Vector2 FallbackCardSize = new(300, 422);

    // Orders are assigned post-draw (see SecondLessonPower), so this overlay always attaches to a
    // card that's already sitting in hand — fading in instead of snapping straight to full opacity
    // is what keeps that from reading as an abrupt pop-in.
    private const float FadeInSeconds = 0.35f;

    private static bool _warmedUp;

    // Forces the shader's GPU pipeline to compile now, off-screen and invisible, instead of at the
    // moment a card first shows the overlay. Call this as soon as SecondLessonPower is granted (see
    // TheSecondLesson.OnPlay) — that's at least a full turn before the earliest a card could
    // actually need the effect (Orders are assigned next turn's AfterPlayerTurnStartLate), giving
    // the one-time compile stall several frames of headroom to finish unnoticed. Idempotent: only
    // the first call in a given game session does anything.
    public static void Schedule()
    {
        if (_warmedUp) return;
        _warmedUp = true;

        var root = ((SceneTree)Engine.GetMainLoop()).Root;
        var rect = new ColorRect
        {
            Material = OverlayMaterial,
            Size = new Vector2(1, 1),
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        root.AddChild(rect);
        rect.CallDeferred(Node.MethodName.QueueFree);
    }

    [HarmonyPostfix]
    public static void Postfix(NCard __instance)
    {
        if (AttachedOverlays.TryGetValue(__instance, out var existing))
        {
            __instance.OverlayContainer.RemoveChild(existing);
            existing.QueueFree();
            AttachedOverlays.Remove(__instance);
        }

        if (__instance.Model?.Affliction is not Order) return;

        var frame = __instance.OverlayContainer.GetParent()?.GetNodeOrNull<Control>("Frame");
        var size = frame?.Size ?? FallbackCardSize;
        var position = frame?.Position ?? -FallbackCardSize / 2f;

        var rect = new ColorRect
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = OverlayMaterial,
            Size = size,
            Position = position,
            Modulate = new Color(1, 1, 1, 0)
        };

        __instance.OverlayContainer.AddChild(rect);
        AttachedOverlays.Add(__instance, rect);
        rect.CreateTween().TweenProperty(rect, "modulate:a", 1.0, FadeInSeconds);

        Log.Info($"OrderOverlayPatch: attached overlay for card={__instance.Model.Id} " +
                  $"frameFound={frame != null} rectSize={rect.Size} rectPosition={rect.Position}");
    }
}
