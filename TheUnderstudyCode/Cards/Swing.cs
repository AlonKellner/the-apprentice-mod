using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Swing : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Swing";

    public Swing() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<SwingPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
