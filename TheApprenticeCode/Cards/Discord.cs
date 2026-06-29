using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Discord : ApprenticeCard
{
    public const string CardId = "TheApprentice:Discord";

    public Discord() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, 2).Execute(context);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, cardPlay.Card);
    }
}
