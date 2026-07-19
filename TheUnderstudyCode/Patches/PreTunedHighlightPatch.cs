using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Color a pre-Tuned card's damage/block number relative to base + 1 (its intrinsic Tuned 1) rather
// than the bare printed base.
//
// The game's DynamicVar.ToHighlightedString colors the previewed number by comparing
// (int)PreviewValue to (int)EnchantedValue (which, absent an enchantment, equals BaseValue):
// higher -> green, lower -> red, equal -> neutral. A pre-Tuned card's printed base is deliberately
// one lower than the value it actually plays at (it starts every combat with Tuned 1), so its
// neutral point should be base + 1, not base. Without this shift a pre-Tuned Practice weakened to
// 0 (base 0 + Tuned 1 = 1, x0.75 Weak, floored) compares 0 vs 0 and renders neutral -- hiding that
// Weak is dropping it below its intended output -- and its ordinary base+1 value renders green as
// if it were buffed.
//
// So, only while ToHighlightedString runs for a pre-Tuned card's damage/block var, bump the
// comparison baseline (EnchantedValue) up by 1, then restore it immediately. Net effect:
//   * below base+1 (including the exact base, e.g. under Weak) -> red
//   * exactly base+1                                           -> neutral
//   * above base+1 (extra Tuned from other cards, Strength...) -> green
// PreviewValue (the number actually shown) is untouched; only its color reference moves.
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.ToHighlightedString))]
public static class PreTunedHighlightPatch
{
    private static readonly AccessTools.FieldRef<DynamicVar, AbstractModel?> OwnerRef =
        AccessTools.FieldRefAccess<DynamicVar, AbstractModel?>("_owner");

    [HarmonyPrefix]
    public static void Prefix(DynamicVar __instance, out decimal? __state)
    {
        __state = null;
        // Only the Tuned-boosted vars (damage/block) get the +1 intrinsic; skip Repeat/self-debuff/etc.
        if (__instance is not (DamageVar or BlockVar)) return;
        if (OwnerRef(__instance) is not UnderstudyCard { IsPreTuned: true }) return;

        __state = __instance.EnchantedValue;   // save whatever baseline the game set (base, or base+enchant)
        __instance.EnchantedValue += 1;        // neutral point = that baseline + intrinsic Tuned 1
    }

    [HarmonyPostfix]
    public static void Postfix(DynamicVar __instance, decimal? __state)
    {
        if (__state.HasValue) __instance.EnchantedValue = __state.Value;
    }
}
