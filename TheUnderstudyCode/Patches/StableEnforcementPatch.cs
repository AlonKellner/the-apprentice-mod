using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Immediate half of the Stable "freeze": CardModifier.ApplyInternal is the single choke point every
// BaseLib modifier addition flows through (AddModifier -> ApplyInternal -> InsertSorted). Postfixing it
// lets a Stable card reject a foreign modifier the instant any source — another card, a base-game effect
// — tries to add one, instead of waiting for the next combat-event hook. Strip-only (see
// UnderstudyCard.RejectForeignModifierIfStable); full reconciliation stays on the hooks.
[HarmonyPatch(typeof(CardModifier), "ApplyInternal")]
public static class StableEnforcementPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModifier __instance, CardModel card)
    {
        // Enforcing guard: Restore re-adds modifiers via AddModifier -> ApplyInternal; don't fight it.
        if (StableEnforcer.Enforcing) return;
        if (card is UnderstudyCard understudy)
            understudy.RejectForeignModifierIfStable(__instance);
    }
}
