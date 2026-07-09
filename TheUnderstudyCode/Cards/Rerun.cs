using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Rerun : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Rerun";

    public Rerun() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithTip(typeof(JadedPower));
        WithVar(new SelfDebuffVar("Jaded", 2));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, 2).Execute(context);
        await EmotionalExpression.ApplyJadedToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Jaded"].BaseValue, this);
    }
}
