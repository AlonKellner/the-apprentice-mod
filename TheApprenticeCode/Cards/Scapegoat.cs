using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Scapegoat : ApprenticeCard
{
    public const string CardId = "TheApprentice:Scapegoat";

    public Scapegoat() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;

        if (IsUpgraded)
        {
            foreach (var enemy in CombatState!.HittableEnemies)
            {
                await EmotionalExpression.TransferWeakTo(context, creature, enemy, cardPlay.Card, 3);
                await EmotionalExpression.TransferVulnerableTo(context, creature, enemy, cardPlay.Card, 3);
            }
        }
        else
        {
            await EmotionalExpression.TransferWeakTo(context, creature, cardPlay.Target!, cardPlay.Card, 3);
            await EmotionalExpression.TransferVulnerableTo(context, creature, cardPlay.Target!, cardPlay.Card, 3);
        }
    }
}
