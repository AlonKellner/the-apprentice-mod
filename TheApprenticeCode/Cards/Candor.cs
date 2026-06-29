using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Candor : ApprenticeCard
{
    public const string CardId = "TheApprentice:Candor";

    public Candor() : base(1, CardType.Skill, CardRarity.Basic, TargetType.AllEnemies)
    {
        WithCostUpgradeBy(-1);
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<VulnerablePower>(context, enemy, 1m, creature, cardPlay.Card, false);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, cardPlay.Card);
    }
}
