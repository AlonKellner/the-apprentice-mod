using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OpenMind : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OpenMind";

    public OpenMind() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(3);
        WithTip(typeof(LimitedPower));
        WithTip(typeof(UnlimitedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await EmotionalExpression.ConvertLimitedToUnlimited(context, creature, 1);
    }
}
