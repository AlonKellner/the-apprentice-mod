using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

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
// combat a pre-Tuned card has no modifier yet but starts each combat with PreTunedStacks Tuned, so it
// previews (PreTunedStacks, PreTunedStacks) — (1, 1) for the usual pre-Tuned card, (2, 2) for Repertoire+.
//
// DamageVar/BlockVar.UpdateCardPreview only runs the damage/block hooks (which inject the full Tuned
// bonus) when runGlobalHooks == true (the active in-hand card). For pile / out-of-combat previews
// (false) the hooks don't run, so we add `total` to the displayed value here.
public static class TunedPreview
{
    // (dynamicBasePart, total) for this card's Tuned state. See the class comment.
    //
    // The pre-Tuned branch is gated to OUT OF COMBAT (CombatState == null): out of combat a pre-Tuned card
    // has no TunedModifier yet but will start combat carrying PreTunedStacks, so it previews (N, N). IN
    // combat the actual TunedModifier is authoritative (first branch) — a pre-Tuned card whose Tuned was
    // stripped mid-combat (e.g. by Spectacle) correctly reports (0, 0). Without this gate the class-level
    // IsPreTuned stays true after the strip, so it kept inflating EnchantedValue and coloring the (correct)
    // base value red.
    public static (int dynamicBasePart, int total) TunedParts(CardModel card) =>
        card.TryGetModifier<TunedModifier>(out var t) ? (t!.Stacks, t.Bonus)
        : card is UnderstudyCard { IsPreTuned: true } uc && card.CombatState == null ? (uc.PreTunedStacks, uc.PreTunedStacks)
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

    // ── Out-of-run surfaces (the Compendium) ────────────────────────────────────────────────────────
    // CardModel.UpdateDynamicVarPreview opens with `if (RunState == null && CombatState == null) return;`
    // so on a card with neither — every Compendium surface — the game runs NO dynamic-var preview and
    // the DamageVar/BlockVar postfixes above never fire. A pre-Tuned card then showed its printed base
    // in the Compendium while showing base+1 in a card reward (which is inside a run, so the preview
    // does run). These two members let TunedCompendiumPreviewPatch close that gap.
    //
    // Deliberately NOT done by lifting the game's early return: that would run every DynamicVar's
    // UpdateCardPreview on a canonical card, and CalculatedVar.Calculate is not gated on runGlobalHooks —
    // it indexes _vars["CalculationBase"]/["CalculationExtra"]/["ExtraDamage"] and throws
    // KeyNotFoundException for any card (ours, base-game, or another mod's) that wires a CalculatedVar
    // loosely. Touching only Damage/Block avoids that entire class of failure.

    // The exact complement of that early return. Safe on a canonical card: RunState and CombatState are
    // both `_owner?.…`, unlike Owner which asserts mutability and throws.
    public static bool ShouldApplyOutOfRun(CardModel card) =>
        card.RunState == null && card.CombatState == null;

    public static void ApplyOutOfRun(IEnumerable<DynamicVar> vars, CardModel card)
    {
        foreach (var v in vars)
        {
            if (v is not (DamageVar or BlockVar)) continue;

            // The reset DamageVar/BlockVar.UpdateCardPreview would have done before we add on top.
            // NCard.UpdateVisuals calls DynamicVars.ClearPreview() first, so that path is already
            // clean — but NCardLibrary's search-text filter calls UpdateDynamicVarPreview with no
            // reset, once per card per keystroke, and Add does `PreviewValue += total`. Without this
            // the Compendium's number climbs 1, 2, 3... as you type.
            v.PreviewValue = v.BaseValue;
            Add(v, card, runGlobalHooks: false);
        }
    }
}

[HarmonyPatch(typeof(DamageVar), nameof(DamageVar.UpdateCardPreview))]
public static class TunedDamagePreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(DamageVar __instance, CardModel card, bool runGlobalHooks)
    {
        decimal beforePreview = __instance.PreviewValue, beforeEnch = __instance.EnchantedValue;
        TunedPreview.Add(__instance, card, runGlobalHooks);
        TunedPreviewDiagnostics.Log("Damage", __instance, card, runGlobalHooks, beforePreview, beforeEnch);
    }
}

[HarmonyPatch(typeof(BlockVar), nameof(BlockVar.UpdateCardPreview))]
public static class TunedBlockPreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(BlockVar __instance, CardModel card, bool runGlobalHooks)
    {
        decimal beforePreview = __instance.PreviewValue, beforeEnch = __instance.EnchantedValue;
        TunedPreview.Add(__instance, card, runGlobalHooks);
        TunedPreviewDiagnostics.Log("Block", __instance, card, runGlobalHooks, beforePreview, beforeEnch);
    }
}

// Temporary diagnostics for the "One-Up previews 2 in the deck view but 1 in hand" bug. Lives here (only
// invoked from the Harmony postfixes above, never from a bare test) so its Log.* calls can't crash the test
// host. Logs only for pre-Tuned / Tuned-carrying cards to limit spam, dumping enough to see which TunedParts
// branch fired and the exact live Tuned-carrier set the Bonus multiplies by.
public static class TunedPreviewDiagnostics
{
    public static void Log(string kind, DynamicVar var, CardModel card, bool runGlobalHooks,
        decimal beforePreview, decimal beforeEnch)
    {
        bool hasMod = card.TryGetModifier<TunedModifier>(out var mod);
        bool preTuned = card is UnderstudyCard { IsPreTuned: true };
        if (!hasMod && !preTuned) return;

        var (dynamicBasePart, total) = TunedPreview.TunedParts(card);

        // Replicate TunedModifier.TunedCardCount's enumeration so we can SEE the carriers it counts.
        string carriers = "n/a";
        int count = -1;
        try
        {
            if (card is { IsMutable: true } && card.Owner is { } player)
            {
                var found = player.Piles.Where(p => p.IsCombatPile).SelectMany(p => p.Cards)
                    .Where(c => c.TryGetModifier<TunedModifier>(out _)).ToList();
                count = found.Count;
                carriers = string.Join(" | ", found.ConvertAll(c =>
                {
                    c.TryGetModifier<TunedModifier>(out var m);
                    return $"{c.GetType().Name}#{c.GetHashCode()}[pile={c.Pile?.Type.ToString() ?? "null"},stacks={m?.Stacks}]";
                }));
            }
        }
        catch (Exception e) { carriers = "err:" + e.Message; }

        MegaCrit.Sts2.Core.Logging.Log.Info(
            $"[TunedPreviewDiag] {kind} {card.GetType().Name}#{card.GetHashCode()} " +
            $"pile={card.Pile?.Type.ToString() ?? "null"} upgPrev={card.UpgradePreviewType} " +
            $"runGlobalHooks={runGlobalHooks} combatNull={card.CombatState == null} runNull={card.RunState == null} " +
            $"hasMod={hasMod} stacks={(hasMod ? mod!.Stacks : 0)} preTuned={preTuned} " +
            $"TunedParts=({dynamicBasePart},{total}) carrierCount={count} carriers=[{carriers}] " +
            $"BaseValue={var.BaseValue} Ench:{beforeEnch}->{var.EnchantedValue} Preview:{beforePreview}->{var.PreviewValue}");
    }
}
