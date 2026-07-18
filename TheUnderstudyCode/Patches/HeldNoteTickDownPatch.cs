using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Held Note — "Turn-based buffs and debuffs no longer decrease by 1 each turn."
//
// PowerCmd.TickDownDuration(PowerModel) is the base game's single shared per-turn decrement gate: it
// checks power.SkipNextDurationTick and otherwise calls Decrement(power). Every base-game duration
// power routes its once-per-turn tick through it (Weak, Vulnerable, Frail, Blur, Intangible,
// Temporary Strength/Dexterity/Focus, Dampen, Shrink, Constrict, Poison count, NoDraw/NoBlock/NoEnergy,
// etc.). Skipping this call when the power's owner has Held Note freezes ALL of them at once, without
// enumerating them.
//
// This only skips the per-turn DECREMENT — other end-of-turn effects (Poison dealing its damage,
// Ritual granting Strength, ...) run in the power's own AfterSideTurnEnd hook, not here, so they are
// unaffected. Permanent (non-duration) powers such as Strength/Dexterity never reach TickDownDuration,
// so the flag is irrelevant to them.
//
// The mod's own invertible powers (Shaken/Jaded/Limited + Un- buffs, Unweak, Unvulnerable, Tension)
// self-decrement in their own turn hook and independently check HeldNotePower.IsActive, so they are
// covered without going through this gate.
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.TickDownDuration))]
public static class HeldNoteTickDownPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PowerModel power, ref Task __result)
    {
        if (!HeldNotePower.IsActive(power.Owner)) return true; // run the normal decrement

        __result = Task.CompletedTask; // owner has Held Note: skip the per-turn decrement
        return false;
    }
}
