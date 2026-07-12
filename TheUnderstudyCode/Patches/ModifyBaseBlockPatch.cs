using BaseLib.Abstracts;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Fills the block-side gap BaseLib left. BaseLib bridges card modifiers into the DAMAGE calculation
// via ModifyBaseDamagePatches (a Harmony prefix on Hook.ModifyDamage that calls
// modifier.ModifyBaseDamageAdditive on cardSource.GetModifiers()), but ships no equivalent for block:
// CardModifier.ModifyBaseBlockAdditive exists as a virtual yet nothing ever invokes it. This mirrors
// the damage patch on Hook.ModifyBlock so a card modifier can modify its card's Block through the same
// version-stable ModifyBase* contract instead of the game's 6-arg AbstractModel.ModifyBlockAdditive
// hook (whose signature drifts between game versions). Tense is the only modifier that uses it today.
//
// The powered-block gate lives here rather than in ModifyBaseBlockAdditive because BaseLib's
// ModifyBaseBlockAdditive(decimal) virtual carries no ValueProp (unlike ModifyBaseDamageAdditive).
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
public static class ModifyBaseBlockPatch
{
    [HarmonyPrefix]
    private static void AdjustBaseAdditive(ref decimal block, ValueProp props, CardModel? cardSource)
    {
        if (cardSource == null || !props.IsPoweredCardOrMonsterMoveBlock()) return;
        foreach (CardModifier modifier in cardSource.GetModifiers())
            block += modifier.ModifyBaseBlockAdditive(block);
    }
}
