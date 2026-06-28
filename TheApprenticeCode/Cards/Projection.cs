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
            await PowerCmd.Apply<WeakPower>(context, enemy, weakAmount + 1, creature, cardPlay.Card, false);
            await PowerCmd.Apply<VulnerablePower>(context, enemy, vulAmount + 1, creature, cardPlay.Card, false);
        }

        if (!IsUpgraded)
            await EmotionalExpression.ApplyWeakToSelf(context, creature, 1, cardPlay.Card);
    }
}
