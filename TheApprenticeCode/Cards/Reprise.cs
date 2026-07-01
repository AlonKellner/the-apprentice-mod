using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Reprise : ApprenticeCard
{
    public const string CardId = "TheApprentice:Reprise";

    public Reprise() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveAllTension(context, creature, cardPlay.Card);
        if (removed <= 0) return;
        int hits = IsUpgraded ? 2 : 1;
        for (int i = 0; i < hits; i++)
            await DamageCmd.Attack(removed)
                .FromCard(cardPlay.Card)
                .TargetingAllOpponents(CombatState!)
                .Execute(context);
    }
}
