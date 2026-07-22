using System.Reflection;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Hides (and currently DIAGNOSES) the "%DebugLabel" that NEpochSlot shows on every slot — including
// locked ones — when the game runs non-release (a modded/beta build does). Earlier attempts silently did
// nothing because the patch class used only METHOD-level [HarmonyPatch] attributes: Harmony's PatchAll
// only discovers classes annotated with [HarmonyPatch] at the CLASS level, so the whole class was skipped.
// These are now two separate classes, each with a class-level attribute. TEMPORARY [EpochDiag] logging;
// strip it (keep the SetState hide) once confirmed. Logs -> SlayTheSpire2/logs/godot.log.

// Tests whether Harmony can patch a Godot lifecycle callback at all.
[HarmonyPatch(typeof(NEpochSlot), "_Ready")]
public static class EpochSlotReadyDiagPatch
{
    private static bool _logged;

    [HarmonyPostfix]
    public static void Postfix()
    {
        if (_logged) return;
        _logged = true;
        MainFile.Logger.Info("[EpochDiag] _Ready postfix RAN (Harmony patched the Godot lifecycle method).");
    }
}

// The real fix: hide the debug label after every state set. Also logs the live IsReleaseGame() value,
// whether the label node is findable, and a one-time node-tree dump to locate it if "%DebugLabel" is wrong.
[HarmonyPatch(typeof(NEpochSlot), nameof(NEpochSlot.SetState))]
public static class EpochSlotSetStatePatch
{
    private static bool _loggedRelease;
    private static bool _dumpedTree;

    [HarmonyPostfix]
    public static void Postfix(NEpochSlot __instance)
    {
        try
        {
            if (!_loggedRelease)
            {
                _loggedRelease = true;
                MainFile.Logger.Info($"[EpochDiag] NGame.IsReleaseGame() = {NGame.IsReleaseGame()}");
            }

            var byUnique = __instance.GetNodeOrNull<Control>("%DebugLabel");
            var byFind = __instance.FindChild("DebugLabel", recursive: true, owned: false) as Control;
            var label = byUnique ?? byFind;

            MainFile.Logger.Info(
                $"[EpochDiag] SetState postfix RAN epoch={SafeId(__instance)} " +
                $"foundByUniqueName={byUnique != null} foundByRecursiveFind={byFind != null} " +
                $"labelVisible={(label != null ? label.Visible.ToString() : "n/a")}");

            if (label != null) label.Visible = false;

            if (!_dumpedTree)
            {
                _dumpedTree = true;
                var sb = new StringBuilder();
                DumpTree(__instance, 0, sb);
                MainFile.Logger.Info("[EpochDiag] Slot node tree (first slot):\n" + sb);
            }
        }
        catch (System.Exception e)
        {
            MainFile.Logger.Error("[EpochDiag] SetState postfix threw: " + e);
        }
    }

    private static void DumpTree(Node node, int depth, StringBuilder sb)
    {
        string indent = new string(' ', depth * 2);
        string vis = node is CanvasItem ci ? $" visible={ci.Visible}" : "";
        string text = "";
        PropertyInfo? textProp = node.GetType().GetProperty("Text");
        if (textProp != null && textProp.PropertyType == typeof(string))
        {
            if (textProp.GetValue(node) is string t && !string.IsNullOrEmpty(t)) text = $" text=\"{t}\"";
        }
        sb.AppendLine($"{indent}{node.Name} [{node.GetType().Name}]{vis}{text}");
        foreach (Node child in node.GetChildren()) DumpTree(child, depth + 1, sb);
    }

    private static string SafeId(NEpochSlot slot)
    {
        try
        {
            object? model = AccessTools.Field(typeof(NEpochSlot), "model")?.GetValue(slot);
            return model?.GetType().GetProperty("Id")?.GetValue(model)?.ToString() ?? "?";
        }
        catch { return "?"; }
    }
}
