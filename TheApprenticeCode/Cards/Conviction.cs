using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Conviction : ApprenticeCard
{
    public const string CardId = "TheApprentice:Conviction";

    public Conviction() : base(1, CardType.Power, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal flag = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<ConvictionPower>(context, creature, flag, creature, cardPlay.Card, false);
    }
}
