using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Dynamics : ApprenticeCard
{
    public const string CardId = "TheApprentice:Dynamics";

    public Dynamics() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.None);
        WithTip(typeof(DynamicsNextCardPower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        decimal amount = IsUpgraded ? 8m : 5m;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<DynamicsNextCardPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
