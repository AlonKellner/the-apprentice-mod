using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The exact analogue of InvertibleBasePowerTooltipPatch, for the "[gold]Swappable[/gold]." suffix.
//
// The base-game swappable debuffs — Weak/Vulnerable/Frail/Poison/Doom/Constrict/Tainted (see
// SceneStealing.SwappableDebuffs) — are sealed vanilla classes with data-driven tooltips, so there's
// no subclass to override and a loc-file override would be global. We append the suffix at runtime via
// a HoverTips postfix, gated on InvertTrackerPower (auto-attached only to an Understudy player, so the
// suffix — exactly like Invertible — appears only when an Understudy deck holding these mechanics is in
// play). The gate is owner-relative: Swap gives YOUR debuffs to enemies, so it's the player's own
// swappable debuffs that are marked. The mod's own swappable debuff, Tension, already carries
// "[gold]Swappable[/gold]." directly in its PowerLoc.
//
// Weak/Vulnerable/Frail are also invertible, so they receive both suffixes (each patch appends its own,
// guarded to stay idempotent regardless of postfix order).
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.HoverTips), MethodType.Getter)]
public static class SwappableBasePowerTooltipPatch
{
    private static readonly PropertyInfo DescriptionProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Description))!;

    private static bool IsSwappableBaseDebuff(PowerModel power) => power switch
    {
        WeakPower or VulnerablePower or FrailPower or PoisonPower or DoomPower or ConstrictPower
            or TaintedPower => true,
        _ => false
    };

    [HarmonyPostfix]
    public static void Postfix(PowerModel __instance, IEnumerable<IHoverTip> __result)
    {
        if (!__instance.IsMutable || !IsSwappableBaseDebuff(__instance)) return;
        if (__instance.Owner.GetPower<InvertTrackerPower>() == null) return;
        if (__result is not List<IHoverTip> tips || tips.Count == 0) return;

        var mainTip = (HoverTip)tips[0];
        if (mainTip.Description.Contains("[gold]Swappable[/gold]")) return;
        DescriptionProperty.SetValue(tips[0], mainTip.Description + " [gold]Swappable[/gold].");
    }
}
