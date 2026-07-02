using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Bravura : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Bravura";

    public Bravura() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(14);
        WithTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, cardPlay.Card);
    }
}
