using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class AnotherBrick : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:AnotherBrick";

    public AnotherBrick() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<AnotherBrickPower>(5, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<AnotherBrickPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
