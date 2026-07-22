using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// In non-release builds, NEpochSlot._Ready makes a "%DebugLabel" (the epoch's class name) visible on
// EVERY slot, including locked ones — a dev affordance gated on !NGame.IsReleaseGame(). A modded game can
// run as non-release, so those class-name labels appear over all epochs (base and Understudy alike),
// spoiling locked names and generally looking wrong. This postfix re-hides the debug label so the
// Timeline reads like the shipping game (names surface only through the normal reveal flow).
[HarmonyPatch(typeof(NEpochSlot), "_Ready")]
public static class EpochSlotDebugLabelPatch
{
    [HarmonyPostfix]
    public static void Postfix(NEpochSlot __instance)
    {
        var label = __instance.GetNodeOrNull<Control>("%DebugLabel");
        if (label != null) label.Visible = false;
    }
}
