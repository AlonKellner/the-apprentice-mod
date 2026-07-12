using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Take Notes grants Vigor "whenever a debuff of yours clears." PowerCmd.Remove is the single async
// choke point every debuff clear passes through, but it fires no broadcast hook (only the removed
// power's own RemoveInternal/AfterRemoved) — so a listening power can't observe someone else's
// debuff clearing, and detection has to happen here. See DebuffClearNotifier for the full rationale.
//
// Async postfix: chain the notification onto the removal's own Task so the Vigor grant is awaited
// in-order within the combat pipeline (e.g. inside Tainted's AfterSideTurnEnd await), rather than
// fired-and-forgotten off a synchronous event. Targets the non-generic Remove(PowerModel); the
// generic Remove<T>(Creature) overload delegates to it, so both are covered.
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), new[] { typeof(PowerModel) })]
public static class DebuffClearOnRemovePatch
{
    [HarmonyPostfix]
    public static void Postfix(PowerModel? power, ref Task __result)
    {
        __result = ContinueAfterRemoval(__result, power);
    }

    private static async Task ContinueAfterRemoval(Task original, PowerModel? power)
    {
        await original;
        await DebuffClearNotifier.NotifyDebuffRemoved(power);
    }
}
