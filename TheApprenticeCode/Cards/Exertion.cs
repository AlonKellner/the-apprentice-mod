using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Exertion : ApprenticeCard
{
    public const string CardId = "TheApprentice:Exertion";

    public Exertion() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = IsUpgraded ? 6 : 4;
        await PowerCmd.Apply<VigorPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
