using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class DeceptiveCadence : ApprenticeCard
{
    public const string CardId = "TheApprentice:DeceptiveCadence";

    public DeceptiveCadence() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(DeceptiveCadencePower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        // Amount=2 on upgraded version signals TensionPower to grant 1 block per Tension held over.
        decimal markerAmount = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<DeceptiveCadencePower>(context, creature, markerAmount, creature, cardPlay.Card, false);
    }
}
