using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Sforzando : ApprenticeCard
{
    public const string CardId = "TheApprentice:Sforzando";

    public Sforzando() : base(0, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithTip(typeof(StrengthPower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        var player = cardPlay.Card.Owner;
        await PowerCmd.Apply<StrengthPower>(context, creature, -1m, creature, cardPlay.Card, false);
        int tensionGain = IsUpgraded ? 5 : 4;
        await TensionHelper.AddTension(context, creature, tensionGain, cardPlay.Card);
        await PlayerCmd.GainEnergy(1, player);
    }
}
