using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Crescendo : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Crescendo";

    public Crescendo() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithPower<CrescendoPower>(1);
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<CrescendoPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
