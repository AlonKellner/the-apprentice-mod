using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Flourish : ApprenticeCard
{
    public const string CardId = "TheApprentice:Flourish";

    public Flourish() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithDamage(7);
        WithTip(typeof(UnvulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await EmotionalExpression.ApplyUnvulnerable(context, creature, 1, cardPlay.Card);
    }
}
