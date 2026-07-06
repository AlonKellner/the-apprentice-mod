using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Weak/Vulnerable/Frail/Strength/Dexterity are sealed vanilla classes whose tooltip text is fully
// data-driven (PowerModel.HoverTips reads a LocString from the "powers" loc table), so there's no
// subclass to override. A loc-file override would work but would be global — every character would
// show [gold]Invertible[/gold], even though the cross-cancellation behavior only exists on a creature carrying
// InvertTrackerPower (auto-attached by UnderstudyCard.AfterPlayerTurnStartLate, i.e. only when the
// owning player's deck actually holds an Understudy card — so this correctly follows the mechanic
// even across events that hand out cards from other characters).
// HoverTip is a sealed `record struct` we don't own, and its Description setter is private, so a
// `with` expression isn't available from outside the type; mutating the boxed instance already
// sitting in the returned list via reflection is what lets us patch it in place.
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.HoverTips), MethodType.Getter)]
public static class InvertibleBasePowerTooltipPatch
{
    private static readonly PropertyInfo DescriptionProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Description))!;

    private static bool IsInvertibleBasePower(PowerModel power) => power switch
    {
        WeakPower or VulnerablePower or FrailPower or StrengthPower or DexterityPower => true,
        _ => false
    };

    [HarmonyPostfix]
    public static void Postfix(PowerModel __instance, IEnumerable<IHoverTip> __result)
    {
        if (!__instance.IsMutable || !IsInvertibleBasePower(__instance)) return;
        if (__instance.Owner.GetPower<InvertTrackerPower>() == null) return;
        if (__result is not List<IHoverTip> tips || tips.Count == 0) return;

        var mainTip = (HoverTip)tips[0];
        DescriptionProperty.SetValue(tips[0], mainTip.Description + " [gold]Invertible[/gold].");
    }
}
