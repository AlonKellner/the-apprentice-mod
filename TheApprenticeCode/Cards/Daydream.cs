using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Daydream : ApprenticeCard
{
    public const string CardId = "TheApprentice:Daydream";

    public Daydream() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithDreamTips();
    }

    public override bool HasExpend => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int cardsInHand = player.Piles
            .Where(p => p.Type == PileType.Hand)
            .SelectMany(p => p.Cards)
            .Count();
        await DreamsAndAmbitions.AddDreams(player, CombatState!, cardsInHand, IsUpgraded);
    }
}
