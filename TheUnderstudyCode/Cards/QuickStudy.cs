using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class QuickStudy : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:QuickStudy";

    public QuickStudy() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(UnweakPower));
    }

    protected override void OnUpgrade() { base.OnUpgrade(); DynamicVars.Cards.UpgradeValueBy(1m); }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ConvertWeakToUnweak(context, creature, 1);
    }
}
