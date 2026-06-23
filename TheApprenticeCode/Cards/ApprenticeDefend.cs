using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ApprenticeDefend : ApprenticeCard
{
    public const string CardId = "TheApprentice:ApprenticeDefend";

    public ApprenticeDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        decimal block = IsUpgraded ? 8m : 5m;
        await CreatureCmd.GainBlock(cardPlay.Card.Owner.Creature, block, ValueProp.Move, cardPlay, false);
    }
}
