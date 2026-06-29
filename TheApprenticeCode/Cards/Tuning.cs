using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tuning : ApprenticeCard
{
    public const string CardId = "TheApprentice:Tuning";

    public Tuning() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(TuningPower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        decimal amount = IsUpgraded ? 2m : 1m;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<TuningPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
