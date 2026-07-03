using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Overexert : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Overexert";

    public Overexert() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(24);
        WithTip(typeof(LimitedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.ApplyLimitedToSelf(context, cardPlay.Card.Owner.Creature, 2, this);
    }
}
