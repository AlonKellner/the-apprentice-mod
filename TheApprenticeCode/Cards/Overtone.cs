using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Overtone : ApprenticeCard
{
    public const string CardId = "TheApprentice:Overtone";

    public Overtone() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int tensionGain = IsUpgraded ? 3 : 2;
        await TensionHelper.AddTension(context, creature, tensionGain, cardPlay.Card);
        await PlayerCmd.GainEnergy(1, cardPlay.Card.Owner);
    }
}
