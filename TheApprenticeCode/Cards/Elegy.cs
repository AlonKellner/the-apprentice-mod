using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Elegy : ApprenticeCard
{
    public const string CardId = "TheApprentice:Elegy";

    public Elegy() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await EmotionalExpression.ApplyVulnerableToSelf(context, cardPlay.Card.Owner.Creature, 1, cardPlay.Card);
    }
}
