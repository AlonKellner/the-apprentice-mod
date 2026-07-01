using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Relics;
using PerformanceCard = TheApprentice.TheApprenticeCode.Cards.Performance;

namespace TheApprentice.TheApprenticeCode.Character;

public class TheApprenticeB : PlaceholderCharacterModel
{
    public const string CharacterId = "TheApprenticeB";

    public override Color NameColor => TheApprentice.Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeB>(),
        ModelDb.Card<StrikeB>(),
        ModelDb.Card<StrikeB>(),
        ModelDb.Card<StrikeB>(),
        ModelDb.Card<DefendB>(),
        ModelDb.Card<DefendB>(),
        ModelDb.Card<DefendB>(),
        ModelDb.Card<DefendB>(),
        ModelDb.Card<PerformanceCard>(),
        ModelDb.Card<Intention>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BlankSlate>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<TheApprenticeBCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheApprenticeRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheApprenticePotionPool>();

    // No art overrides — inherits PlaceholderCharacterModel's "ironclad" placeholder assets.
    // Replace with custom assets when this character graduates from prototype status.
}
