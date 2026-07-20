using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Enchantments;

// Persistent marker applied out of combat by the Score relic: a deck card carrying it starts each
// combat pre-Planned. No per-card state is stored here — the actual deck-ordered Planned slot is
// assigned at combat start by PrePlannedSetup (which treats this enchantment as pre-Planned). The
// engine persists the enchantment on the master-deck card across combats and saves; only one
// enchantment fits per card, so a card can be Scored or Foldable-Staged, not both (base CanEnchant
// enforces this). Loc in enchantments.json (THEUNDERSTUDY-PRE_PLANNED.*).
public class PrePlanned : CustomEnchantmentModel
{
    public override bool CanEnchant(CardModel card) =>
        base.CanEnchant(card) && PlannedModifier.CanApplyTo(card);
}
