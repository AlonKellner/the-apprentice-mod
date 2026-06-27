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
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;

        if (IsUpgraded)
        {
            foreach (var enemy in CombatState!.HittableEnemies)
                await EmotionalExpression.TransferDebuffsTo(context, creature, enemy, cardPlay.Card);
        }
        else
        {
            await EmotionalExpression.TransferDebuffsTo(context, creature, cardPlay.Target!, cardPlay.Card);
        }
    }
}
