using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Fortissimo : ApprenticeCard
{
    public const string CardId = "TheApprentice:Fortissimo";

    public Fortissimo() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(typeof(FortissimoPower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Amount=2 on upgrade signals FortissimoPower to also triple Tension gains.
        decimal amount = IsUpgraded ? 2m : 1m;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<FortissimoPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
