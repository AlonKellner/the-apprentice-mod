using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Dynamic base: color and display a card's damage/block relative to (printed base + the card's own
// Tuned amount), not the bare printed base.
//
// The game colors a card's number by comparing (int)PreviewValue to (int)EnchantedValue in
// DynamicVar.ToHighlightedString (which the {Damage:diff()} card-text token writes). EnchantedValue is
// the game's "part of the card, not colored like a buff" baseline (defaults to BaseValue). So we
// decompose a Tuned card's value into two parts:
//   * dynamicBasePart = the card's OWN Tuned amount (Stacks, the "Tuned N" shown on it). Folded into
//     EnchantedValue -> it's the color-neutral baseline ("used as the base value in practice").
//   * total           = the card's full Tuned bonus (Stacks * TunedCreated). Added to the displayed
//     value. The surplus over the dynamic base (total - Stacks, i.e. the extra from other Tuned cards'
//     multiplier) therefore colors green like Strength, and external modifiers (Weak/Strength/...)
//     color on top as usual.
//
// This is NOT pre-Tuned gated -- it's driven by whatever Tuned the card currently carries. Out of
// combat a pre-Tuned card has no modifier yet but starts each combat Tuned 1, so it previews (1, 1).
//
// DamageVar/BlockVar.UpdateCardPreview only runs the damage/block hooks (which inject the full Tuned
// bonus) when runGlobalHooks == true (the active in-hand card). For pile / out-of-combat previews
// (false) the hooks don't run, so we add `total` to the displayed value here.
public static class TunedPreview
{
    // (dynamicBasePart, total) for this card's Tuned state. See the class comment.
    public static (int dynamicBasePart, int total) TunedParts(CardModel card) =>
        card.TryGetModifier<TunedModifier>(out var t) ? (t!.Stacks, t.Bonus)
        : card is UnderstudyCard { IsPreTuned: true } ? (1, 1)
        : (0, 0);

    public static void Add(DynamicVar var, CardModel card, bool runGlobalHooks)
    {
        var (dynamicBasePart, total) = TunedParts(card);
        if (dynamicBasePart == 0 && total == 0) return;

        // Dynamic base = the game's "part of the card" baseline + the card's own Tuned amount.
        // Recompute from a stable baseline each call so repeated previews stay idempotent
        // (EnchantedValue has no reset of its own for un-enchanted cards).
        decimal baseline = card.Enchantment != null ? var.EnchantedValue : var.BaseValue;
        var.EnchantedValue = baseline + dynamicBasePart;

        // Displayed value: the active in-hand preview already had the full Tuned bonus applied by the
        // hook; pile & out-of-combat previews (no hooks ran) add it here.
        if (!runGlobalHooks) var.PreviewValue += total;
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
