using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class AllNighter : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:AllNighter";

    public AllNighter() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithCards(1);
        WithTip(typeof(JadedPower));
        WithVar(new SelfDebuffVar("Jaded", 1));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ApplyJadedToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Jaded"].BaseValue, this);
    }
}
