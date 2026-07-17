using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Vigor is invertible in this mod via SIGN (like Strength/Dexterity), rather than through a separate
// "Un-" power — the mod's convention is that every Un-X power is a buff, so there is no debuff-side
// "Unvigor" class. Negative Vigor reduces the owner's next Attack (VigorPower.ModifyDamageAdditive
// already returns base.Amount, which is negative when Vigor is negative). The only thing stopping
// that is PowerModel.AllowNegative defaulting to false, which clamps Vigor at 0. This patch flips
// AllowNegative to true for VigorPower only, so Invert's sign-flip and Swap can produce negative
// Vigor exactly the way they already do for Strength/Dexterity (which are AllowNegative in vanilla).
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.AllowNegative), MethodType.Getter)]
public static class VigorAllowNegativePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PowerModel __instance, ref bool __result)
    {
        if (__instance is not VigorPower) return true;
        __result = true;
        return false;
    }
}
