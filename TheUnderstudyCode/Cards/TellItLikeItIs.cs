using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TellItLikeItIs : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TellItLikeItIs";

    public TellItLikeItIs() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithBlock(8);
        WithTip(typeof(WeakPower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        var creature = cardPlay.Card.Owner.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        await PowerCmd.Apply<WeakPower>(context, cardPlay.Target!, amount, creature, cardPlay.Card, false);
    }
}
