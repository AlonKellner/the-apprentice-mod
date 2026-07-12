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
[HarmonyPatch(typeof(HoverTipFactory), nameof(HoverTipFactory.FromKeyword))]
public static class PlannedKeywordIconPatch
{
    private static readonly PropertyInfo IconProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Icon))!;

    // The canonical PlannedCounterPower's own icon (planned_counter_power.png via its
    // CustomPackedIconPath). Loaded once; ResourceLoader caching makes repeat access cheap anyway.
    private static Texture2D? _icon;

    [HarmonyPostfix]
    public static void Postfix(CardKeyword keyword, IHoverTip __result)
    {
        if (keyword != UnderstudyKeywords.Planned) return;
        if (__result is not HoverTip tip || tip.Icon != null) return;
        _icon ??= ModelDb.Power<PlannedCounterPower>().Icon;
        IconProperty.SetValue(__result, _icon);
    }
}
