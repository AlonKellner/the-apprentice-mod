using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FreezeUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FreezeUp";

    public FreezeUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(17);
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.ApplyWeakToSelf(context, cardPlay.Card.Owner.Creature, 1, this);
    }
}
