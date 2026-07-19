using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The global recency observer behind Swap's "most recently modified" selection. Hook.AfterPowerAmountChanged
// is the single chokepoint every real (non-zero) power-amount change funnels through — both the new-application
// path and the stacking/ModifyAmount path in PowerCmd — and it fires for EVERY creature, not just the player.
// So one postfix here records the modification order for the player and every enemy uniformly, with no hidden
// power to attach to each enemy and no spawn-timing gap. SceneStealing.Swap reads SwapRecency to pick each
// creature's latest swappable debuff/buff.
//
// Side (debuff vs buff) is deliberately NOT decided here: we record recency per power id only, and the give/take
// selection decides candidacy from the power's current amount at play time (a sign-flip Vigor counts as a debuff
// while negative, a buff while positive), so a single recency stamp serves whichever side it currently is.
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPowerAmountChanged))]
public static class SwapRecencyPatch
{
    [HarmonyPrefix]
    public static void Prefix(PowerModel power)
    {
        // Owner (set by ApplyInternal before this hook fires) throws on a canonical model, so gate on IsMutable.
        if (!power.IsMutable) return;
        var owner = power.Owner;
        if (owner == null) return;
        string entry = power.Id.Entry;
        if (!SceneStealing.IsSwappableEntry(entry)) return;
        SwapRecency.Record(owner, entry);
    }
}
