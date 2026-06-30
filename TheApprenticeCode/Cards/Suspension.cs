using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Suspension : ApprenticeCard
{
    public const string CardId = "TheApprentice:Suspension";

    public Suspension() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        decimal amount = IsUpgraded ? 12m : 8m;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<SuspensionPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
