using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Temporary placeholder art for the 7 Understudy epochs. Their real portraits would live in the game's
// shared image space (slot thumbnail = the `epoch_atlas` sprite via EpochModel.Portrait; inspect view =
// res://images/timeline/epoch_portraits/<id>.png via RealPortrait). Baking those requires Godot-imported
// assets in the shared res://images tree; until that art exists, these prefixes short-circuit our epochs'
// portrait getters to an already-imported mod texture so the slots render something instead of a blank
// rect (and without the failed-load error spam a missing path would produce).
//
// TO REMOVE WHEN REAL EPOCH ART SHIPS: delete this file (or gate it on ResourceLoader.Exists of the real
// paths) so the baked atlas/png art is used instead.
internal static class UnderstudyEpochPortrait
{
    private const string PlaceholderPath = "res://TheUnderstudy/images/charui/character_icon_the_understudy.png";
    private static Texture2D? _placeholder;

    internal static bool IsUnderstudyEpoch(EpochModel model) =>
        model.Id.StartsWith("THEUNDERSTUDY", StringComparison.Ordinal);

    internal static Texture2D? Placeholder
    {
        get
        {
            if (_placeholder != null) return _placeholder;
            try { _placeholder = ResourceLoader.Load<Texture2D>(PlaceholderPath); }
            catch (Exception e) { MainFile.Logger.Error("Understudy epoch placeholder art failed to load: " + e); }
            return _placeholder;
        }
    }
}

[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.Portrait), MethodType.Getter)]
internal static class UnderstudyEpochSlotPortraitPatch
{
    [HarmonyPrefix]
    private static bool Prefix(EpochModel __instance, ref Texture2D __result)
    {
        if (!UnderstudyEpochPortrait.IsUnderstudyEpoch(__instance)) return true;
        __result = UnderstudyEpochPortrait.Placeholder!;
        return false;
    }
}

[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.RealPortrait), MethodType.Getter)]
internal static class UnderstudyEpochInspectPortraitPatch
{
    [HarmonyPrefix]
    private static bool Prefix(EpochModel __instance, ref Texture2D __result)
    {
        if (!UnderstudyEpochPortrait.IsUnderstudyEpoch(__instance)) return true;
        __result = UnderstudyEpochPortrait.Placeholder!;
        return false;
    }
}
