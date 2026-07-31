using BaseLib.Abstracts;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Fills the block-side gap BaseLib left. BaseLib bridges card modifiers into the DAMAGE calculation
// via ModifyBaseDamagePatches (a Harmony prefix on Hook.ModifyDamage that calls
// modifier.ModifyBaseDamageAdditive on cardSource.GetModifiers()), but ships no equivalent for block —
// and its CardModifier.ModifyBaseBlockAdditive virtual was never invoked, then in 3.4.x marked
// [Obsolete("Not currently functional")] with a changed signature (a ValueProp param added). Calling
// that virtual from here crashed with MissingMethodException once players' Workshop BaseLib drifted
// ahead of the mod's compile-time version. So this patch does NOT touch that virtual: it reads Tuned's
// Bonus off the card's own TunedModifier directly, keeping Tuned's block bonus off the drifting API.
// Tuned is the only modifier that adds Block today; if another is ever added, extend the check here.
//
// The powered-block gate lives here (props is available on this hook).
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
public static class ModifyBaseBlockPatch
{
    [HarmonyPrefix]
    private static void AdjustBaseAdditive(ref decimal block, ValueProp props, CardModel? cardSource)
    {
        if (cardSource == null || !props.IsPoweredCardOrMonsterMoveBlock()) return;
        foreach (CardModifier modifier in cardSource.GetModifiers())
            if (modifier is TunedModifier tuned)
                block += tuned.Bonus;
    }
}
