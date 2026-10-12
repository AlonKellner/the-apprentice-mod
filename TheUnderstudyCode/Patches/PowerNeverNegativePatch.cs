using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Spiny Toad and Toadpole give themselves Thorns, then a later move takes the same amount away with a
// NEGATIVE apply (`Apply<ThornsPower>(-5)`). If Swap stole the Thorns in between, the power is gone, and
// a negative apply to a missing power goes down PowerCmd.Apply's "fresh instance" path, which, unlike
// ModifyAmount, never checks ShouldRemoveDueToAmount. So the toad ends up with -5 Thorns: it can't get
// Thorns from its next Spikes (+5 just cancels it out), and its negative Thorns misfires when it's hit.
//
// Rule: a power that doesn't AllowNegative can never be CREATED with a negative amount. Taking away what
// isn't there is a no-op, so the stat stays at zero. A negative apply to an EXISTING instance is
// untouched: ModifyAmount already removes the power once it reaches 0 or below. The rule is general on
// purpose: the base game only ever applies negative amounts to AllowNegative powers (Strength,
// Dexterity, Focus, Shrink) and these Thorns cleanups, so nothing legitimate goes through the blocked path.
//
// Patches the non-generic overload; the generic Apply<T> calls it for every fresh instance.
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), new[]
{
    typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature), typeof(decimal),
    typeof(Creature), typeof(CardModel), typeof(bool),
})]
public static class PowerNeverNegativePatch
{
    // Pure: whether this apply would create a fresh power below zero that isn't allowed to be negative.
    public static bool ShouldDrop(decimal amount, bool allowNegative, bool hasExistingInstance) =>
        amount < 0m && !allowNegative && !hasExistingInstance;

    [HarmonyPrefix]
    public static bool Prefix(PowerModel power, Creature target, decimal amount, Creature? applier, ref Task __result)
    {
        if (amount >= 0m || power.AllowNegative) return true;
        bool exists = PowerCmd.FindExistingInstanceForStacking(power, target, applier) != null;
        if (!ShouldDrop(amount, power.AllowNegative, exists)) return true;
        __result = Task.CompletedTask;
        return false;
    }
}
