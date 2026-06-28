using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class TrueStrength : ApprenticeCard
{
    public const string CardId = "TheApprentice:TrueStrength";

    public TrueStrength() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(UnweakPower));
        WithTip(typeof(UnvulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal amount = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<TrueStrengthPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
