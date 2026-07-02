using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Rallentando : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Rallentando";

    public Rallentando() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(3);
        WithTip(typeof(WeakPower));
        WithTip(typeof(UnweakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await EmotionalExpression.ConvertWeakToUnweak(context, creature, 1);
    }
}
