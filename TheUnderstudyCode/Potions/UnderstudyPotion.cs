using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Shared base for The Understudy's potions. The [Pool] attribute is Inherited=true, so every concrete
// potion below auto-joins the character's potion pool (same mechanism as UnderstudyCard/relics — no
// pool list to edit). All of these act on the player / their own deck, so they take no reticle target
// (TargetType.None) and read base.Owner (the Player) rather than the passed target. Loc lives in the
// mod's potions.json (keys THEUNDERSTUDY-<NAME>.title/.description/.selectionPrompt), matching the
// relics.json/cards.json convention; art is a follow-up (falls back to the game's missing-potion sprite).
[Pool(typeof(TheUnderstudyPotionPool))]
public abstract class UnderstudyPotion : CustomPotionModel
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.None;
}
