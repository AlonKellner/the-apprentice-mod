using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Base-game TaintedPower.AfterSideTurnEnd removes the power on `side == CombatSide.Enemy`, hardcoded
// on the assumption that Tainted only ever lives on the player (self-debuff exploited during the
// enemy's attack turn, then cleared). Swap can now hand Tainted to enemies, and for an enemy owner
// that removal would fire at the end of the enemy's OWN turn — clearing it before the player ever
// gets to attack the tainted enemy.
//
// Fix without a transpiler: for an enemy-owned Tainted, flip the `side` argument (Player<->Enemy)
// before the original runs. The base `== Enemy` check then resolves relative to the owner — i.e.
// "remove at the end of the opponent's turn, after the owner was attacked" — which is exactly the
// player-owned behavior, mirrored. Player-owned Tainted is left untouched. Tainted's damage half is
// already owner-relative (target != Owner guard) and needs no patch.
[HarmonyPatch(typeof(TaintedPower), nameof(TaintedPower.AfterSideTurnEnd))]
public static class TaintedSwappablePatch
{
    [HarmonyPrefix]
    public static void Prefix(TaintedPower __instance, ref CombatSide side)
    {
        if (!__instance.IsMutable) return;
        if (__instance.Owner.Side != CombatSide.Enemy) return;
        side = side == CombatSide.Enemy ? CombatSide.Player : CombatSide.Enemy;
    }
}
