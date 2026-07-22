using System.Reflection;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Hides (and, right now, DIAGNOSES) the "%DebugLabel" that NEpochSlot shows on every slot — including
// locked ones — when the game runs non-release (a modded game does). The plain hide via SetState wasn't
// taking, so this build also logs, tagged "[EpochDiag]", to answer:
//   - does the SetState postfix run at all? does the _Ready postfix run? (Harmony + Godot lifecycle)
//   - what does NGame.IsReleaseGame() return in the live modded build?
//   - can we find the label node (via unique name vs recursive find), and what is its type/visibility?
//   - the full node tree of a slot, to locate the actual label if it isn't "%DebugLabel".
// TEMPORARY: strip the logging (keep the SetState hide) once the cause is known.
public static class EpochSlotDebugLabelPatch
{
    private static bool _loggedReleaseFlag;
    private static bool _loggedReady;
    private static bool _dumpedTree;

    [HarmonyPatch(typeof(NEpochSlot), "_Ready")]
    [HarmonyPostfix]
    public static void ReadyPostfix(NEpochSlot __instance)
    {
        if (_loggedReady) return;
        _loggedReady = true;
        MainFile.Logger.Info("[EpochDiag] _Ready postfix RAN (Harmony patched the Godot lifecycle method).");
    }

    [HarmonyPatch(typeof(NEpochSlot), nameof(NEpochSlot.SetState))]
    [HarmonyPostfix]
    public static void SetStatePostfix(NEpochSlot __instance)
    {
        try
        {
            if (!_loggedReleaseFlag)
            {
                _loggedReleaseFlag = true;
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
