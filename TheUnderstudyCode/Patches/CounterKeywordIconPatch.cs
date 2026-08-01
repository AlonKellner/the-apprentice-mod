using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// HoverTipFactory.FromKeyword builds a keyword's hover tip with no icon (BaseLib's own FromKeyword
// prefix does the same when synthesizing a custom keyword's tip), so the Planned and Tuned keyword
// tooltips have no image by default. Attach each keyword's matching counter-power icon
// (PlannedCounterPower / TunedCounterPower) to its tip here: WithTip(Planned) / WithTip(Tuned) route
// through FromKeyword, so every card and combat surface that shows the keyword picks it up.
//
// Tuned has a SECOND surface this doesn't cover: a card actually carrying Tuned shows TunedModifier's
// own stack-aware "Tuned N." tip (built directly, not via FromKeyword — TunedModifier.AddTips removes
// the FromKeyword one). That tip is given the same icon at its construction site in TunedModifier.AddTips.
//
// HoverTip is a `record struct` whose Icon setter is private and which BaseLib may rebuild fresh each
// call, so mutate the boxed instance in-place via reflection (same technique as
// InvertibleBasePowerTooltipPatch) rather than trying to construct a replacement.
//
// Deliberately holds NO cached Texture2D. The game unloads room assets between rooms ("Unloading N
// missed cache assets"), which disposes the underlying Godot resource — and a `static Texture2D`
// primed in one combat then hands a disposed object to every later hover, throwing
// ObjectDisposedException out of NHoverTipSet.Init. Re-resolving is cheap: PowerModel.Icon is itself
// just ResourceLoader.Load(..., CacheMode.Reuse), so a live texture comes back from the cache and a
// dead one is reloaded.
//
// The staleness also has to be repaired, not merely avoided: HoverTipFactory.FromKeyword memoizes one
// HoverTip per keyword, and this postfix mutates that cached instance, so a disposed icon baked into it
// would otherwise be served for the rest of the process. Hence the validity test rather than a plain
// "already has an icon" early-out.
[HarmonyPatch(typeof(HoverTipFactory), nameof(HoverTipFactory.FromKeyword))]
public static class CounterKeywordIconPatch
{
    private static readonly PropertyInfo IconProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Icon))!;

    [HarmonyPostfix]
    public static void Postfix(CardKeyword keyword, IHoverTip __result)
    {
        if (__result is not HoverTip tip) return;
        if (tip.Icon != null && GodotObject.IsInstanceValid(tip.Icon)) return;

        // The canonical counter power's own icon (its CustomPackedIconPath: planned_counter_power.png /
        // tension_power.png). null for any other keyword — leave those untouched.
        Texture2D? icon =
            keyword == UnderstudyKeywords.Planned ? ModelDb.Power<PlannedCounterPower>().Icon
            : keyword == UnderstudyKeywords.Tuned ? ModelDb.Power<TunedCounterPower>().Icon
            : null;
        if (icon == null) return;

        IconProperty.SetValue(__result, icon);
    }
}
