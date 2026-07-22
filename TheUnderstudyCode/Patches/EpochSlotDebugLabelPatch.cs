using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// In non-release builds, NEpochSlot._Ready makes a "%DebugLabel" (the epoch's class name) visible on
// EVERY slot, including locked ones — a dev affordance gated on !NGame.IsReleaseGame(). A modded game runs
// as non-release, so those class-name labels appear over all epochs (base and Understudy alike), spoiling
// locked names.
//
// We hook SetState rather than _Ready: Harmony reliably patches ordinary methods, but patching Godot
// lifecycle callbacks like _Ready is flaky (the engine dispatches them through generated invokers). SetState
// runs at the end of _Ready and again whenever a slot's state changes, so re-hiding the debug label here is
// both reliable and self-healing. Scoped to the epoch slots only, so the game's other dev features are left
// intact.
[HarmonyPatch(typeof(NEpochSlot), nameof(NEpochSlot.SetState))]
public static class EpochSlotDebugLabelPatch
{
    [HarmonyPostfix]
    public static void Postfix(NEpochSlot __instance)
    {
        var label = __instance.GetNodeOrNull<Control>("%DebugLabel")
                    ?? __instance.FindChild("DebugLabel", recursive: true, owned: false) as Control;
        if (label != null) label.Visible = false;
    }
}
