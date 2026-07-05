using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class SteadyNow : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SteadyNow";

    public SteadyNow() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(9);
        WithTip(UnderstudyKeywords.Invert);
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.InvertLastModified(context, cardPlay.Card.Owner.Creature, 2);
    }
}
