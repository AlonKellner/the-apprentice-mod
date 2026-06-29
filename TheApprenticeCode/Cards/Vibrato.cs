using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Vibrato : ApprenticeCard
{
    public const string CardId = "TheApprentice:Vibrato";

    public Vibrato() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int tension = TensionHelper.GetTension(creature);
        if (tension <= 0) return;
        decimal damage = IsUpgraded ? tension : (tension + 1) / 2m;  // full or ceil(half)
        await DamageCmd.Attack(damage)
            .FromCard(cardPlay.Card)
            .TargetingAllOpponents(CombatState!)
            .Execute(context);
        // Tension is NOT removed
    }
}
