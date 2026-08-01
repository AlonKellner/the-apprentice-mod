using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Potions;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The potion counterpart of RelicOutlineOverride. The Understudy potions ship no outline art, so
// PotionModel.OutlinePath resolves to a non-existent potion_outline_atlas entry (returns null) and the
// Potion Lab's colored-shadow branch (NLabPotionHolder, for Visible potions) has no Outline texture to
// tint gold. BaseLib exposes no potion-image override (unlike RelicImageOverridePatch), so patch
// PotionModel.OutlinePath directly: for an Understudy potion with no real outline, hand back the same
// generic outline shape the relics use, which the Potion Lab then tints to the character's gold. Only the
// outline is affected — the potion image stays at the default placeholder until real art exists.
[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OutlinePath), MethodType.Getter)]
public static class PotionOutlineOverride
{
    [HarmonyPostfix]
    public static void Postfix(PotionModel __instance, ref string? __result)
    {
        if (__result == null && __instance is UnderstudyPotion)
            __result = MainFile.ResPath + "/images/generic_outline.png";
    }
}
