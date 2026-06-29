using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Breakthrough : ApprenticeCard
{
    public const string CardId = "TheApprentice:Breakthrough";

    public Breakthrough() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(UnvulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int cap = IsUpgraded ? 2 : 1;
        await EmotionalExpression.ConvertVulnerableToUnvulnerable(context, creature, cap);
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
