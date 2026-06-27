using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Fortitude : ApprenticeCard
{
    public const string CardId = "TheApprentice:Fortitude";

    public Fortitude() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(WeakPower));
        // VulnerablePower only applies when upgraded — use a conditional lambda so the
        // unupgraded card doesn't carry a tooltip for a keyword not in its description.
        TooltipSource upgradedVul = typeof(VulnerablePower);
        WithTip(new TooltipSource(card => card.IsUpgraded ? upgradedVul.Tip(card) : null!));
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal amount = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<FortitudePower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
