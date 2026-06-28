using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Extensions;
using TheApprentice.TheApprenticeCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TheApprentice.TheApprenticeCode.Character;

public class TheApprentice : PlaceholderCharacterModel
{
    public const string CharacterId = "TheApprentice";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<ApprenticeStrike>(),
        ModelDb.Card<ApprenticeStrike>(),
        ModelDb.Card<ApprenticeStrike>(),
        ModelDb.Card<ApprenticeStrike>(),
        ModelDb.Card<ApprenticeDefend>(),
        ModelDb.Card<ApprenticeDefend>(),
        ModelDb.Card<ApprenticeDefend>(),
        ModelDb.Card<ApprenticeDefend>(),
        ModelDb.Card<Plan>(),
        ModelDb.Card<JustAsPlanned>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ConstantStruggle>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<TheApprenticeCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheApprenticeRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheApprenticePotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_the_apprentice.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_the_apprentice.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_the_apprentice_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_the_apprentice.png".CharacterUiPath();
}
