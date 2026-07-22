using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using TheUnderstudy.TheUnderstudyCode.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Makes all 7 Understudy epochs reachable from the Timeline root (Neow) at once — exactly as Neow
// already exposes every Ironclad epoch. GetRevealableEpochs BFS-walks GetTimelineExpansion outward from
// Neow; appending ours here means each becomes revealable the moment its own condition obtains it, with
// no enforced order between them (soft reveal — driven purely by which condition the player meets first).
// Our own epochs' GetTimelineExpansion stay empty (they're leaves).
[HarmonyPatch(typeof(NeowEpoch), nameof(NeowEpoch.GetTimelineExpansion))]
public static class UnderstudyEpochReachabilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref EpochModel[] __result)
    {
        __result = __result.Concat(new EpochModel[]
        {
            EpochModel.Get<Understudy1Epoch>(),
            EpochModel.Get<Understudy2Epoch>(),
            EpochModel.Get<Understudy3Epoch>(),
            EpochModel.Get<Understudy4Epoch>(),
            EpochModel.Get<Understudy5Epoch>(),
            EpochModel.Get<Understudy6Epoch>(),
            EpochModel.Get<Understudy7Epoch>(),
        }).ToArray();
    }
}
