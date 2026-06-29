using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Strain : ApprenticeCard
{
    public const string CardId = "TheApprentice:Strain";

    public Strain() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(StrengthPower));
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(context, creature, -1m, creature, cardPlay.Card, false);
        int vigor = IsUpgraded ? 8 : 5;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, cardPlay.Card, false);
    }
}
