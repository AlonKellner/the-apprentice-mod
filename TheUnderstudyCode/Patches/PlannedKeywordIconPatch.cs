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
// prefix does the same when synthesizing a custom keyword's tip), so the Planned keyword tooltip has
// no image by default. Attach the PlannedCounterPower icon to the Planned tip here: WithTip(Planned)
// routes through FromKeyword, so every card and combat surface that shows the Planned keyword picks
// it up. HoverTip is a `record struct` whose Icon setter is private and which BaseLib may rebuild
// fresh each call, so mutate the boxed instance in-place via reflection (same technique as
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
public static class PlannedKeywordIconPatch
{
    private static readonly PropertyInfo IconProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Icon))!;

    [HarmonyPostfix]
    public static void Postfix(CardKeyword keyword, IHoverTip __result)
    {
        if (keyword != UnderstudyKeywords.Planned) return;
        if (__result is not HoverTip tip) return;
        if (tip.Icon != null && GodotObject.IsInstanceValid(tip.Icon)) return;

        // The canonical PlannedCounterPower's own icon (planned_counter_power.png via its
        // CustomPackedIconPath).
        IconProperty.SetValue(__result, ModelDb.Power<PlannedCounterPower>().Icon);
    }
}
