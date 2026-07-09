using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OffScript : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OffScript";

    public OffScript() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        WithDamage(4);
        WithTip(typeof(LimitedPower));
        WithVar(new SelfDebuffVar("Limited", 1));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, 3).Execute(context);
        await EmotionalExpression.ApplyLimitedToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Limited"].BaseValue, this);
    }
}
