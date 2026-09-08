using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Route an Ascension-10 double-boss alt FIRST boss to its chained alt SECOND boss instead of the ending.
//
// After a boss reward, NRewardsScreen.OnProceedButtonPressed opens the map to travel to the second boss
// ONLY when you're standing on the DEFAULT first boss:
//     if (Map.SecondBossMapPoint != null && CurrentMapCoord == Map.BossMapPoint.coord)
//         ProceedFromTerminalRewardsScreen();   // open the map -> travel to the second boss
//     else
//         ActChangeSynchronizer.SetLocalPlayerReady();   // -> EnterNextAct (last act = Architect ending)
//
// A Book of Endings alt first boss sits at its own injected coord, never Map.BossMapPoint.coord, so on a
// double-boss act the base check was false and the run jumped straight to the Architect after the alt
// first boss — the chained alt second boss (injected directly above it) was skipped entirely. Confirmed
// in the logs: won AEONGLASS_BOSS (the alt first boss at (6,14)) -> "ready to move to next act" -> Architect,
// with (6,15) -> the chained second never entered.
//
// This prefix recognizes exactly that missed case — a terminal boss reward standing on an alt first boss
// that still has a chained alt second boss — and does what the base game does for the default first boss:
// open the map so the player can travel to the second boss (AltBossTravelabilityPatch makes it clickable),
// skipping the base method's move-to-next-act path. Every other case (default boss, alt single boss, the
// alt SECOND boss itself, non-boss rooms) returns true and runs the base method unchanged — so the alt
// second boss still ends the run exactly like the default second boss does.
[HarmonyPatch(typeof(NRewardsScreen), "OnProceedButtonPressed")]
public static class AltBossSecondBossProceedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NRewardsScreen __instance)
    {
        var rm = RunManager.Instance;
        if (rm == null) return true;
        // The base game's debug-rewards override takes precedence in the original; never pre-empt it.
        if (rm.debugAfterCombatRewardsOverride != null) return true;

        var t = Traverse.Create(__instance);
        if (!t.Field("_isTerminal").GetValue<bool>()) return true;

        var runState = t.Field("_runState").GetValue<IRunState>();
        if (runState?.CurrentRoom is not { RoomType: RoomType.Boss }) return true; // boss rooms only
        if (runState.CurrentMapCoord is not { } coord) return true;
        if (!AltBossStore.HasChainedSecondBoss(runState.Map, coord)) return true; // the only case we plug

        Log.Info($"[BookOfEndings] alt first boss ({coord.col},{coord.row}) cleared; opening map to its " +
                 "chained alt second boss instead of moving to the next act");
        TaskHelper.RunSafely(rm.ProceedFromTerminalRewardsScreen());
        return false; // skip the base move-to-next-act path
    }
}
