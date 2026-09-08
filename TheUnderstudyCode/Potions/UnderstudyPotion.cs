using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Shared base for The Understudy's potions. The [Pool] attribute is Inherited=true, so every concrete
// potion below auto-joins the character's potion pool (same mechanism as UnderstudyCard/relics — no
// pool list to edit). All of these act on the player / their own deck, so they take no reticle target
// (TargetType.None) and read base.Owner (the Player) rather than the passed target. Loc lives in the
// mod's potions.json (keys THEUNDERSTUDY-<NAME>.title/.description/.selectionPrompt), matching the
// relics.json/cards.json convention.
//
// Art resolves drop-in the same way cards and powers do: images/potions/<slug>.png and
// images/potions/outline/<slug>.png, where the slug is Id.Entry minus the THEUNDERSTUDY- prefix,
// lowercased. Both resolvers return null until the file is packed, and CustomPotionModel's own default
// for these members is null, so an art-less potion keeps the game's missing-potion sprite exactly as
// before. See art/assets/RELICS_POTIONS.md.
[Pool(typeof(TheUnderstudyPotionPool))]
public abstract class UnderstudyPotion : CustomPotionModel
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.None;

    private string ArtFileName => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    public override string? CustomPackedImagePath => ArtFileName.PotionImagePath();

    public override string? CustomPackedOutlinePath => ArtFileName.PotionOutlineImagePath();
}
