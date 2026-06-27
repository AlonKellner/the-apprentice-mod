using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Projection : ApprenticeCard
{
    public const string CardId = "TheApprentice:Projection";

    public Projection() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int weakAmount = creature.GetPowerAmount<WeakPower>();
        int vulAmount = creature.GetPowerAmount<VulnerablePower>();

        foreach (var enemy in CombatState!.HittableEnemies)
        {
            if (weakAmount > 0)
                await PowerCmd.Apply<WeakPower>(context, enemy, weakAmount, creature, cardPlay.Card, false);
            if (vulAmount > 0)
                await PowerCmd.Apply<VulnerablePower>(context, enemy, vulAmount, creature, cardPlay.Card, false);
        }

        if (!IsUpgraded)
            await EmotionalExpression.ApplyWeakToSelf(context, creature, 1, cardPlay.Card);
    }
}
