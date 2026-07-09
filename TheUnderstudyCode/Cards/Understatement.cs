using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Understatement : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Understatement";

    public Understatement() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(12);
        WithInvertibleTip(typeof(WeakPower));
        WithVar(new SelfDebuffVar("Weak", 2));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.ApplyWeakToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Weak"].BaseValue, this);
    }
}
