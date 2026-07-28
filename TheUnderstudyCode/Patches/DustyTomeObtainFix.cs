using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The DustyTome instance obtained from the Darv ancient node is not the one SetupForPlayer previewed on:
// AncientCard is a [SavedProperty] (ModelId?) that comes back null on the freshly-created obtained
// instance, so AfterObtained crashed on ModelDb.GetById(null). If AncientCard is unset when the relic is
// obtained, set it up now — SetupForPlayer routes through BaseLib's ITomeCard path and picks One Take for
// the Understudy. Guarded on null so base-game characters (whose obtained instance already carries a
// chosen ancient) are untouched.
[HarmonyPatch(typeof(DustyTome), "AfterObtained")]
public static class DustyTomeObtainFix
{
    [HarmonyPrefix]
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.AncientCard == null && __instance.Owner != null)
            __instance.SetupForPlayer(__instance.Owner);
    }
}
