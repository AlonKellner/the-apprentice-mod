using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Catharsis : ApprenticeCard
{
    public const string CardId = "TheApprentice:Catharsis";

    public Catharsis() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int weakStacks = creature.GetPowerAmount<WeakPower>();
        int vulStacks = creature.GetPowerAmount<VulnerablePower>();
        int totalStacks = weakStacks + vulStacks;

        if (weakStacks > 0)
            await PowerCmd.Apply<WeakPower>(context, creature, -weakStacks, creature, cardPlay.Card, false);
        if (vulStacks > 0)
            await PowerCmd.Apply<VulnerablePower>(context, creature, -vulStacks, creature, cardPlay.Card, false);

        if (totalStacks > 0)
        {
            decimal damagePerStack = IsUpgraded ? 10m : 8m;
            decimal totalDamage = totalStacks * damagePerStack;
            await DamageCmd.Attack(totalDamage).FromCard(cardPlay.Card).TargetingAllOpponents(CombatState!).Execute(context);
        }
    }
}
