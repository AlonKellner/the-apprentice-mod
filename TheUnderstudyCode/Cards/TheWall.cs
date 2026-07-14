using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TheWall : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TheWall";

    public TheWall() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(10);
        WithInvertibleTip(typeof(VulnerablePower));
        WithVar(new SelfDebuffVar("Vulnerable", 1));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await EmotionalExpression.ApplyVulnerableToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Vulnerable"].BaseValue, this);
    }
}
