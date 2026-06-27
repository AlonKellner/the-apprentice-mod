using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Recrimination : ApprenticeCard
{
    public const string CardId = "TheApprentice:Recrimination";

    public Recrimination() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        // Base: applies Vulnerable stacks. Upgrade: also applies Weak stacks.
        TooltipSource upgradedWeak = typeof(WeakPower);
        WithTip(new TooltipSource(card => card.IsUpgraded ? upgradedWeak.Tip(card) : null!));
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal amount = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<RecriminationPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
