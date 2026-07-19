using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Show Tuned's damage/block bonus in the card preview everywhere it's relevant, not just on the card
// actively being played.
//
// DamageVar/BlockVar.UpdateCardPreview only runs the game's damage/block hooks (which is where Tuned's
// bonus is injected, via BaseLib's ModifyBaseDamagePatches / our ModifyBaseBlockPatch) when
// runGlobalHooks is true — i.e. for the single active in-hand preview. Cards sitting in the draw or
// discard pile are previewed with runGlobalHooks == false, so their numbers would otherwise show the
// bare printed base, ignoring the Tuned stacks they're currently carrying. That makes it hard to see
// how much a Tuned card will actually hit/block for.
//
// So, only when the base preview did NOT run the hooks (runGlobalHooks == false):
//   * In combat (mutable card): add the card's live Tuned bonus, Stacks × (# distinct Tuned cards this
//     combat) — the same formula the in-hand hooks apply — so pile previews match the real value,
//     scaling with every Tuned card currently in the deck.
//   * Out of combat (canonical/ownerless card in deck/reward/library): a pre-Tuned card has no live
//     modifier yet, so add the single Tuned stack it will start each combat with, so it still previews
//     its intended value (base + 1).
internal static class TunedPreview
{
    public static void Add(DynamicVar var, CardModel card, bool runGlobalHooks)
    {
        if (runGlobalHooks) return; // active in-hand preview already applied the bonus via the hooks

        if (card.IsMutable)
        {
            if (card.TryGetModifier<TunedModifier>(out var tuned))
                var.PreviewValue += tuned!.Stacks * TunedModifier.TunedCreated;
        }
        else if (card is UnderstudyCard { IsPreTuned: true })
        {
            var.PreviewValue += 1;
        }
    }
}

[HarmonyPatch(typeof(DamageVar), nameof(DamageVar.UpdateCardPreview))]
public static class TunedDamagePreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(DamageVar __instance, CardModel card, bool runGlobalHooks) =>
        TunedPreview.Add(__instance, card, runGlobalHooks);
}

[HarmonyPatch(typeof(BlockVar), nameof(BlockVar.UpdateCardPreview))]
public static class TunedBlockPreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(BlockVar __instance, CardModel card, bool runGlobalHooks) =>
        TunedPreview.Add(__instance, card, runGlobalHooks);
}
