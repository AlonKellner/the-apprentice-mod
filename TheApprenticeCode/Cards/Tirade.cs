using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tirade : ApprenticeCard
{
    public const string CardId = "TheApprentice:Tirade";

    public Tirade() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        WithDamage(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int blockRemoved = creature.Block;
        await CreatureCmd.LoseBlock(creature, blockRemoved);

        await CommonActions.CardAttack(cardPlay.Card, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(context);

        int threshold = IsUpgraded ? 12 : 15;
        if (blockRemoved >= threshold)
        {
            decimal bonusDamage = IsUpgraded ? 25m : 20m;
            await DamageCmd.Attack(bonusDamage).FromCard(cardPlay.Card).TargetingAllOpponents(CombatState!).Execute(context);
        }
    }
}
