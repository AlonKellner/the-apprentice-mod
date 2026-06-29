using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Cadence : ApprenticeCard
{
    public const string CardId = "TheApprentice:Cadence";

    public Cadence() : base(3, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(typeof(CadencePower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Amount=2 on upgrade signals CadencePower to also draw a card on Tension removal.
        decimal amount = IsUpgraded ? 2m : 1m;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<CadencePower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
