using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Obsession : ApprenticeCard
{
    public const string CardId = "TheApprentice:Obsession";

    public Obsession() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<ObsessionPower>(context, creature, IsUpgraded ? 6m : 4m, creature, cardPlay.Card, false);
    }
}
