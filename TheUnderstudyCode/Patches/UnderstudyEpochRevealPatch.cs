using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using Understudy = TheUnderstudy.TheUnderstudyCode.Character.TheUnderstudy;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Obtains The Understudy's Timeline epochs on the SAME progress conditions base characters use, reusing
// the same `Progress` stats the base checks read (so the conditions stay "pure"). The base checks
// hardcode `character is Ironclad/...` and throw for unknown characters, so BaseLib skips them for
// custom characters — we replicate the equivalent checks here for The Understudy.
//
// Reveal order is NOT enforced (all 7 are reachable at once, see UnderstudyEpochReachabilityPatch); it
// is softly encouraged by the difficulty of each condition. Mapping (condition -> epoch, deliberately
// non-chronological to land the twists):
//   Finish a run       -> A Perfect Mirror   (3)
//   Beat Act 1         -> Dreamless           (1)
//   Beat Act 2         -> Designing an End    (2)
//   Beat Act 3         -> The Final Lesson    (7)
//   Defeat 15 Bosses   -> The Boy in the City (5)
//   Defeat 15 Elites   -> Nothing Like Him    (6)
//   Complete Ascension 1 -> Consumed          (4)
//
// NOTE: the two hooks live in SEPARATE classes each with a CLASS-LEVEL [HarmonyPatch]. Harmony's
// PatchAll only discovers classes annotated at the class level — a single class holding both hooks with
// only method-level attributes is silently skipped (which had disabled this whole feature until v1.11.53).
internal static class UnderstudyEpochReveal
{
    internal const string PerfectMirror   = "THEUNDERSTUDY3_EPOCH"; // finish a run
    internal const string EndingDesigned  = "THEUNDERSTUDY2_EPOCH"; // beat Act 2
    internal const string FinalLesson     = "THEUNDERSTUDY7_EPOCH"; // beat Act 3
    internal const string Dreamless       = "THEUNDERSTUDY1_EPOCH"; // beat Act 1
    internal const string BoyInTheCity    = "THEUNDERSTUDY5_EPOCH"; // 15 Bosses
    internal const string NothingLikeHim  = "THEUNDERSTUDY6_EPOCH"; // 15 Elites
    internal const string Consumed        = "THEUNDERSTUDY4_EPOCH"; // Ascension 1+

    internal static void Obtain(ProgressState progress, Player player, string epochId)
    {
        if (progress.IsEpochObtained(epochId)) return;
        progress.ObtainEpoch(epochId);
        if (!player.DiscoveredEpochs.Contains(epochId)) player.DiscoveredEpochs.Add(epochId);
    }

    internal static void Obtain(ProgressState progress, SerializablePlayer player, string epochId)
    {
        if (progress.IsEpochObtained(epochId)) return;
        progress.ObtainEpoch(epochId);
        if (!player.DiscoveredEpochs.Contains(epochId)) player.DiscoveredEpochs.Add(epochId);
    }

    // Sum the character's wins across a set of encounter ids — the same counting the base
    // CheckFifteen{Bosses,Elites}DefeatedEpoch methods do from Progress.EncounterStats.
    internal static int CountWins(ProgressState progress, ModelId characterId, HashSet<ModelId> encounterIds)
    {
        int total = 0;
        foreach (var encounter in progress.EncounterStats.Values)
        {
            if (!encounterIds.Contains(encounter.Id)) continue;
            foreach (var fight in encounter.FightStats)
            {
                if (fight.Character == characterId)
                {
                    total += fight.Wins;
                    break;
                }
            }
        }
        return total;
    }

    private static HashSet<ModelId>? _bossEncounterIds;
    internal static HashSet<ModelId> BossEncounterIds =>
        _bossEncounterIds ??= ModelDb.Acts.SelectMany(a => a.AllBossEncounters.Select(e => e.Id)).ToHashSet();

    private static HashSet<ModelId>? _eliteEncounterIds;
    internal static HashSet<ModelId> EliteEncounterIds =>
        _eliteEncounterIds ??= ModelDb.AllEncounters.Where(e => e.RoomType == RoomType.Elite).Select(e => e.Id).Distinct().ToHashSet();
}

// Mid-run: act-boss clears + cumulative 15 Bosses / 15 Elites. Postfixes the public entry point
// (the inner base Check* calls are what BaseLib skips, not this method), so it still runs for us.
[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.UpdateAfterCombatWon))]
public static class UnderstudyEpochMidRunRevealPatch
{
    [HarmonyPostfix]
    public static void Postfix(Player localPlayer, CombatRoom room)
    {
        try
        {
            if (localPlayer.Character is not Understudy) return;
            if (localPlayer.RunState.GameMode.AreAchievementsAndEpochsLocked()) return;

            var progress = SaveManager.Instance.Progress;
            var charId = localPlayer.Character.Id;

            if (room.RoomType == RoomType.Boss)
            {
                string? actEpoch = room.CombatState.RunState.CurrentActIndex switch
                {
                    0 => UnderstudyEpochReveal.Dreamless,      // beat Act 1
                    1 => UnderstudyEpochReveal.EndingDesigned, // beat Act 2
                    2 => UnderstudyEpochReveal.FinalLesson,    // beat Act 3
                    _ => null,
                };
                if (actEpoch != null) UnderstudyEpochReveal.Obtain(progress, localPlayer, actEpoch);

                if (UnderstudyEpochReveal.CountWins(progress, charId, UnderstudyEpochReveal.BossEncounterIds) >= 15)
                    UnderstudyEpochReveal.Obtain(progress, localPlayer, UnderstudyEpochReveal.BoyInTheCity);
            }
            else if (room.RoomType == RoomType.Elite)
            {
                if (UnderstudyEpochReveal.CountWins(progress, charId, UnderstudyEpochReveal.EliteEncounterIds) >= 15)
                    UnderstudyEpochReveal.Obtain(progress, localPlayer, UnderstudyEpochReveal.NothingLikeHim);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Error("UnderstudyEpoch mid-run reveal failed: " + e);
        }
    }
}

// Post-run: finish a run (win or lose) + complete Ascension 1. Postfixes the private post-run method
// so we receive the already-resolved local SerializablePlayer (no platform/net-id resolution needed).
[HarmonyPatch(typeof(ProgressSaveManager), "UpdateEpochsPostRun")]
public static class UnderstudyEpochPostRunRevealPatch
{
    [HarmonyPostfix]
    public static void Postfix(SerializablePlayer serializablePlayer, SerializableRun serializableRun, bool victory)
    {
        try
        {
            if (serializableRun.GameMode.AreAchievementsAndEpochsLocked()) return;
            if (serializablePlayer.CharacterId is not { } characterId) return;
            if (ModelDb.GetById<CharacterModel>(characterId) is not Understudy) return;

            var progress = SaveManager.Instance.Progress;

            UnderstudyEpochReveal.Obtain(progress, serializablePlayer, UnderstudyEpochReveal.PerfectMirror); // finishing the run at all
            if (victory && serializableRun.Ascension >= 1)
                UnderstudyEpochReveal.Obtain(progress, serializablePlayer, UnderstudyEpochReveal.Consumed);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error("UnderstudyEpoch post-run reveal failed: " + e);
        }
    }
}
