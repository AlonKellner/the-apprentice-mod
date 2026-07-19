using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// One postfix on PowerModel.HoverTips that appends "[gold]Invertible[/gold]." and/or
// "[gold]Swappable[/gold]." to the live power-icon tooltip of the base-game powers the Understudy's
// mechanics act on.
//
// This used to be two separate Harmony postfixes on the same getter (one per suffix). Two independent
// postfixes each re-reading and reflection-mutating the SAME boxed record-struct HoverTip is fragile
// — in practice Swappable failed to appear on powers that were also Invertible (e.g. Weak, which is
// both). Merging into a single postfix makes the append deterministic and idempotent: each applicable
// suffix is added exactly once, Invertible before Swappable, so Weak/Vulnerable/Frail show both.
//
// Gated on the owner carrying InvertTrackerPower (auto-attached only to an Understudy player), so the
// suffixes appear only when an Understudy deck holding these mechanics is in play, and owner-relative
// (Invert/Swap act on YOUR debuffs). The invertible set (Weak/Vulnerable/Frail/Strength/Dexterity)
// mirrors EmotionalExpression.IdentifyPair; the swappable set (Weak/Vulnerable/Frail/Poison/Doom/
// Constrict/Tainted) mirrors SceneStealing.SwappableDebuffs minus Tension, whose own PowerLoc already
// carries "[gold]Swappable[/gold]." directly.
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.HoverTips), MethodType.Getter)]
public static class BasePowerTooltipSuffixPatch
{
    private const string InvertibleTag = "[gold]Invertible[/gold]";
    private const string SwappableTag = "[gold]Swappable[/gold]";

    private static readonly PropertyInfo DescriptionProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Description))!;

    private static bool IsInvertible(PowerModel p) =>
        p is WeakPower or VulnerablePower or FrailPower or StrengthPower or DexterityPower;

    private static bool IsSwappable(PowerModel p) =>
        p is WeakPower or VulnerablePower or FrailPower or PoisonPower or DoomPower or ConstrictPower
            or TaintedPower;

    // The suffix text this base power still needs, given its current tooltip description (so repeated
    // getter calls stay idempotent). Invertible is appended before Swappable, so a power that is both
    // gets a stable "... Invertible. Swappable." order. Pure/testable.
    public static string MissingSuffix(PowerModel power, string currentDescription)
    {
        string add = "";
        if (IsInvertible(power) && !currentDescription.Contains(InvertibleTag)) add += $" {InvertibleTag}.";
        if (IsSwappable(power) && !currentDescription.Contains(SwappableTag)) add += $" {SwappableTag}.";
        return add;
    }

    [HarmonyPostfix]
    public static void Postfix(PowerModel __instance, IEnumerable<IHoverTip> __result)
    {
        if (!__instance.IsMutable) return;
        if (!IsInvertible(__instance) && !IsSwappable(__instance)) return;
        if (__instance.Owner.GetPower<InvertTrackerPower>() == null) return;
        if (__result is not List<IHoverTip> tips || tips.Count == 0) return;

        var mainTip = (HoverTip)tips[0];
        string add = MissingSuffix(__instance, mainTip.Description);
        if (add.Length == 0) return;

        // Mutate the boxed record-struct already in the list (its Description setter is private).
        // Target tips[0] (the box), never a (HoverTip) cast copy, or the mutation is lost.
        DescriptionProperty.SetValue(tips[0], mainTip.Description + add);
    }
}
