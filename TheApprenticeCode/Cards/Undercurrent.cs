using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Undercurrent : ApprenticeCard
{
    public const string CardId = "TheApprentice:Undercurrent";

    public Undercurrent() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal amount = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<UndercurrentPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
