using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StageFright : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StageFright";

    public StageFright() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(12);
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await EmotionalExpression.ApplyWeakToSelf(context, creature, 1, cardPlay.Card);
    }
}
