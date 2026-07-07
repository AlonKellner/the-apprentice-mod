using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Matinee : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Matinee";

    public Matinee() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(10);
        WithTip(typeof(JadedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.ApplyJadedToSelf(context, cardPlay.Card.Owner.Creature, 1, this);
    }
}
