using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// StableEnforcer freezes what it can snapshot (modifiers, local keywords, replay count) and reverts drift
// after the fact. Two mid-combat modifications can't be handled that way, so they are blocked at the
// game's own gate instead — see the individual patches below.

// Afflictions. Applying one is effectively one-way (ClearAffliction is a separate, deliberate effect, and
// AfflictInternal wants a mutable affliction model), so reverting is not an option. AfflictionModel.CanAfflict
// is the right gate: CardCmd.Afflict consults it before applying, AND AfflictionModel's own random-target
// selection filters candidates through it — so an enemy picks a different card rather than wasting the
// affliction on a Stable one.
//
// This is what makes Stable mean something against the base game: TangledPower (Entangled), HexPower
// (Hexed), ChainsOfBindingPower (Bound), VitalSparkPower (Tainted), RingingPower and GalvanicPower all
// afflict the player's cards mid-combat, and none of them know Stable exists. The mod's own Order already
// checks IsStable() in its CanAfflict override; this generalises that to every affliction.
[HarmonyPatch(typeof(AfflictionModel), nameof(AfflictionModel.CanAfflict))]
public static class StableAfflictionPatch
{
    // IsStable() reads only Keywords + DirectModifiers — never Owner — so it is safe on any card this
    // patch sees, including canonical models and other characters' cards.
    public static bool ShouldBlock(CardModel card) => card.IsStable();

    [HarmonyPostfix]
    public static void Postfix(CardModel card, ref bool __result)
    {
        if (__result && ShouldBlock(card)) __result = false;
    }
}

// In-combat upgrades. Genuinely irreversible: UpgradeInternal/FinalizeUpgradeInternal rewrite the card's
// dynamic vars, cost, keywords and description, and CurrentUpgradeLevel's setter is private and refuses to
// go past MaxUpgradeLevel — there is no downgrade to restore to. So it has to be prevented.
//
// Both CardCmd.Upgrade overloads funnel here (the single-card one wraps its argument in a one-element list
// and calls this), so filtering the enumerable covers every caller: Armaments, Quasar, Storm of Steel,
// Compact, Primal Force, Drain, Knife Trap, Largesse, Charge.
//
// Gated on the card being in a COMBAT pile. Upgrading at a rest site, from an event, or via a relic is not
// a mid-combat modification and must keep working — otherwise a Stable card could never be upgraded at all.
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), new[] { typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle) })]
public static class StableUpgradePatch
{
    public static bool ShouldBlock(CardModel card) => ShouldBlock(card, card.Pile?.Type.IsCombatPile() == true);

    public static bool ShouldBlock(CardModel card, bool inCombatPile) => inCombatPile && card.IsStable();

    [HarmonyPrefix]
    public static void Prefix(ref IEnumerable<CardModel> cards)
    {
        // Materialise before filtering: the argument is often a lazy query over a pile that the upgrade
        // loop itself mutates.
        var kept = cards.Where(c => !ShouldBlock(c)).ToList();
        cards = kept;
    }
}
