using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Penance : ApprenticeCard
{
    public const string CardId = "TheApprentice:Penance";

    public Penance() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        int blockAmount = IsUpgraded ? 10 : 6;
        await PowerCmd.Apply<PenancePower>(context, creature, blockAmount, creature, cardPlay.Card, false);
    }
}
