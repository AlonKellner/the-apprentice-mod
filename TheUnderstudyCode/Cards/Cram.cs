using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Cram in a lot fast — and pay for it with Limited. (Limited self-debuff downside.)
public class Cram : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Cram";

    public Cram() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(3);
        WithTip(typeof(LimitedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ApplyLimitedToSelf(context, creature, 2, this);
    }
}
