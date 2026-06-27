using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Release : ApprenticeCard
{
    public const string CardId = "TheApprentice:Release";

    public Release() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(4);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int blockRemoved = creature.Block;
        await CreatureCmd.LoseBlock(creature, blockRemoved);

        int threshold = IsUpgraded ? 10 : 12;
        decimal bonusDamage = IsUpgraded ? 22m : 18m;

        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        if (blockRemoved >= threshold)
            await DamageCmd.Attack(bonusDamage).FromCard(cardPlay.Card).Targeting(cardPlay.Target!).Execute(context);
    }
}
