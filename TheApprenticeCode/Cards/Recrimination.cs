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
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<RecriminationPower>(context, creature, 1m, creature, cardPlay.Card, false);
    }
}
