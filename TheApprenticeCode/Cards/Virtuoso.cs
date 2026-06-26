using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Virtuoso : ApprenticeCard
{
    public const string CardId = "TheApprentice:Virtuoso";

    public Virtuoso() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VirtuosoPower>(context, creature, IsUpgraded ? 1m : 0m, creature, cardPlay.Card, false);
    }
}
