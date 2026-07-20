using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Enchantments;

// Persistent marker applied out of combat by the Foldable Stage relic: a deck card carrying it starts
// each combat with Tuned. The Tuned is applied at combat start by PrePlannedSetup (which scans for
// this enchantment), so it works for any card type, not only UnderstudyCards. One enchantment per
// card (base CanEnchant), so a card can be Foldable-Staged or Scored, not both. Loc in
// enchantments.json (THEUNDERSTUDY-PRE_TUNED.*).
public class PreTuned : CustomEnchantmentModel
{
    public override bool CanEnchant(CardModel card) =>
        base.CanEnchant(card) && TunedModifier.CanApplyTo(card);
}
