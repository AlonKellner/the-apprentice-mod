using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class RollWithIt : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:RollWithIt";

    public RollWithIt() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
        await EmotionalExpression.InvertLastModified(context, cardPlay.Card.Owner.Creature, 2);
    }
}
