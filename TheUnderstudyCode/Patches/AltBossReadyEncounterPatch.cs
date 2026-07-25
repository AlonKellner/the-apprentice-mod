using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// NBossMapPoint._Ready builds a boss node's art from `base.Point == Map.SecondBossMapPoint ?
// Act.SecondBossEncounter : Act.BossEncounter` — it ignores the point, so a flank node would show the
// DEFAULT boss's art. Rather than mutate the node after the fact (re-initialising the spine leaves the
// shader material momentarily null and crashes RefreshColorInstantly), we surgically change only the one
// value _Ready reads wrong: the transpiler redirects the else-branch `get_BossEncounter` through a
// resolver that returns this flank's assigned encounter for an alt boss, and the real default boss
// otherwise. Everything else in _Ready (fresh MegaSprite, single SetSkeletonDataRes -> valid material,
// RefreshColorInstantly) runs exactly as the game intends, so the flank gets clean full boss art.
//
// The SecondBossMapPoint (true) branch is short-circuited for our nodes (a flank is never the second
// boss point), so only the else branch is redirected; the real default/second boss nodes are unaffected.
[HarmonyPatch(typeof(NBossMapPoint), "_Ready")]
public static class AltBossReadyEncounterPatch
{
    private static readonly MethodInfo GetBossEncounter =
        AccessTools.PropertyGetter(typeof(ActModel), nameof(ActModel.BossEncounter));

    private static readonly MethodInfo Resolver =
        AccessTools.Method(typeof(AltBossReadyEncounterPatch), nameof(ResolveBoss));

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            if (ins.Calls(GetBossEncounter))
            {
                // Stack top is the ActModel from get_Act; push `this` (the node) and swap the property
                // read for our resolver: (ActModel, NBossMapPoint) -> EncounterModel.
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, Resolver);
            }
            else
            {
                yield return ins;
            }
        }
    }

    // For an alt boss, the encounter that flank leads to; otherwise the act's real default boss.
    public static EncounterModel ResolveBoss(ActModel act, NBossMapPoint node)
    {
        var runState = Traverse.Create(node).Field("_runState").GetValue<RunState>();
        if (runState?.Map is { } map)
        {
            var encId = AltBossStore.EncounterAt(map, node.Point.coord);
            if (encId != null)
            {
                var enc = act.AllBossEncounters.FirstOrDefault(e => e.Id.ToString() == encId);
                if (enc != null) return enc;
            }
        }
        return act.BossEncounter;
    }
}
