using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DoubleTime : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DoubleTime";

    public DoubleTime() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPowerNoTip<DoubleTimePower>(1, 1);
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<DoubleTimePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
