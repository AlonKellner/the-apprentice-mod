using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Give an injected alt boss its own boss encounter. RunManager.CreateRoom builds a boss room as
// `(model as EncounterModel) ?? Act.PullNextEncounter(Boss)`, and its args don't say which node is
// being entered — but the entry flow calls AddVisitedMapCoord(coord) before CreateRoom, so
// State.CurrentMapCoord is the node just stepped onto (true for both normal and debug travel). When
// that node is an alt boss, substitute the specific boss AltBossPlan assigned to it (stored per-coord
// in AltBossStore), so the far-left and far-right nodes lead to two different bosses.
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

        var encounterId = AltBossStore.EncounterAt(state.Map, coord);
        if (encounterId == null) return; // not an alt boss — leave the default boss flow alone

        var act = state.Act;
        var encounter = act.AllBossEncounters.FirstOrDefault(e => e.Id.ToString() == encounterId);
        if (encounter == null)
        {
            Log.Warn($"[BookOfOrder] alt boss ({coord.col},{coord.row}) assigned {encounterId} " +
                     $"but it is not in the act's boss pool; falling back to default");
            return;
        }

        model = encounter.ToMutable();
        Log.Info($"[BookOfOrder] entering alt boss ({coord.col},{coord.row}) -> encounter {encounter.Id} " +
                 $"(default boss is {act.BossEncounter.Id})");
    }
}
