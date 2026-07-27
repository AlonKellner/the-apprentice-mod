using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Make the Compendium show a pre-Tuned card the same number every other view shows.
//
// CardModel.UpdateDynamicVarPreview opens with `if (RunState == null && CombatState == null) return;`,
// so on a card with neither the game runs NO dynamic-var preview at all and TunedPreviewPatch's
// DamageVar/BlockVar postfixes never fire. A card reward is inside a run (RunState != null) so it
// previews normally — which is why Practice read "Deal 1 damage" in the Compendium and "Deal 2 damage"
// in a reward, while BOTH screens printed the "Tuned 1." line. The Compendium was contradicting itself.
//
// A Harmony POSTFIX still runs when the original early-returns, so nothing has to be bypassed. The gate
// is the exact complement of that early return: when the method really did run, the DamageVar/BlockVar
// postfixes already applied the bonus and this must not apply it a second time.
//
// One patch covers every no-run render path — the compendium grid (canonical models straight out of
// ModelDb.AllCards), the card-detail inspect screen and the grid's upgrade preview (MutableClones with
// a null owner), and NCardLibrary's search-text filter — because they all funnel through this method.
//
// Deliberately NOT implemented by lifting the early return: that would run every DynamicVar's
// UpdateCardPreview on a canonical card, and CalculatedVar.Calculate is not gated on runGlobalHooks.
// It indexes _vars["CalculationBase"]/["CalculationExtra"]/["ExtraDamage"] and would throw
// KeyNotFoundException for any card — ours, base-game, or another mod's — that wires a CalculatedVar
// loosely, crashing the Compendium on Fate Knocking / Clean Slate / Loosen Up to fix Practice.
[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview))]
public static class TunedCompendiumPreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, DynamicVarSet dynamicVarSet)
    {
        if (!TunedPreview.ShouldApplyOutOfRun(__instance)) return;
        TunedPreview.ApplyOutOfRun(dynamicVarSet.Values, __instance);
    }
}
