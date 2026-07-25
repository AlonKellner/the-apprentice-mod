using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Spike B: give an injected alt boss its own encounter instead of the default. RunManager.CreateRoom
// builds a boss room as `(model as EncounterModel) ?? Act.PullNextEncounter(Boss)`, and its args don't
// say which node is being entered — but the entry flow calls AddVisitedMapCoord(coord) before
// CreateRoom, so State.CurrentMapCoord is the node just stepped onto (true for both normal and debug
// travel). When that node is an alt boss, substitute a different boss from the act's pool as `model`.
//
// SPIKE simplification: picks "any boss that isn't the default". Phase 3 assigns left/right encounters
// deterministically (seeded) with the double-boss derangement; this just proves per-node encounters work.
[HarmonyPatch(typeof(RunManager), "CreateRoom")]
public static class AltBossEncounterPatch
{
    [HarmonyPrefix]
    public static void Prefix(RunManager __instance, RoomType roomType, ref AbstractModel? model)
    {
        if (roomType != RoomType.Boss || model != null) return;

        // State is a private property on RunManager; read it via Traverse.
        var state = Traverse.Create(__instance).Property("State").GetValue<RunState>();
        if (state?.CurrentMapCoord is not { } coord) return;
        if (!AltBossStore.IsAltBoss(state.Map, coord)) return;

        var act = state.Act;
        var alt = act.AllBossEncounters.FirstOrDefault(e => e.Id != act.BossEncounter.Id);
        if (alt == null) return;

        model = alt.ToMutable();
        Log.Info($"[BookOfOrder] entering alt boss ({coord.col},{coord.row}) -> encounter {alt.Id} " +
                 $"(default boss is {act.BossEncounter.Id})");
    }
}
