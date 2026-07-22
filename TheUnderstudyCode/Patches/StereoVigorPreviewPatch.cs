using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// While the card's owner has Stereo (doubles all Vigor they give), any card's "Vigor" var previews at
// double — so the player sees the real number before playing. Cards phrase the var with {Vigor:diff()}
// for gains (surplus colors green) and {Vigor:inverseDiff()} for losses (surplus colors red), exactly
// like damage/block modifier previews. The doubling mirrors StereoPower.ModifyPowerAmountGivenMultiplicative,
// which doubles the actually-applied Vigor at play time, so preview and result agree.
//
// The card-preview loop calls UpdateCardPreview on every DynamicVar (base impl is empty for a plain
// IntVar), so a postfix on the base method reaches the Vigor var. We recompute from EnchantedValue (the
// card's own baseline) each call, so repeated previews stay idempotent.
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.UpdateCardPreview))]
public static class StereoVigorPreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(DynamicVar __instance, CardModel card)
    {
        if (__instance.Name != "Vigor" || !card.IsMutable) return;

        Creature? owner = card.Owner?.Creature;
        int stacks = (int)(owner?.GetPowerAmount<StereoPower>() ?? 0m);
        if (stacks <= 0) return;

        __instance.PreviewValue = __instance.EnchantedValue * StereoPower.ComputeMultiplier(stacks);
    }
}
