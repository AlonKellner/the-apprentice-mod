using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DrawingBlanks : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DrawingBlanks";

    public DrawingBlanks() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(3);
        WithTip(typeof(LimitedPower));
        WithVar(new SelfDebuffVar("Limited", 2));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ApplyLimitedToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Limited"].BaseValue, this);
    }
}
