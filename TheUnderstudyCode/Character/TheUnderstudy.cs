using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Relics;
using PerformanceCard = TheUnderstudy.TheUnderstudyCode.Cards.Performance;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudy : PlaceholderCharacterModel
{
    public const string CharacterId = "TheUnderstudy";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<PerformanceCard>(),
        ModelDb.Card<Buildup>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ConstantStruggle>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<TheUnderstudyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheUnderstudyRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheUnderstudyPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    // Bespoke face art: the in-game top-panel corner portrait (CustomIconTexturePath -> IconTexture)
    // and the character-select face (locked + unlocked) all use Understudy_Face.png. CustomIcon above
    // wraps CustomIconTexturePath, so it picks this up too. The character-select background is a small
    // scene wrapping the Attack card placeholder art as a full-rect TextureRect — CharacterSelectBg is
    // instantiated as a Control, so it must be a scene, not a plain texture. The map marker is left as
    // the inherited Ironclad default until dedicated art is made.
    public override string CustomIconTexturePath => "character_icon_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_the_understudy_locked.png".CharacterUiPath();
    public override string CustomCharacterSelectBg => "char_select_bg_the_understudy.tscn".SceneResPath();
}
