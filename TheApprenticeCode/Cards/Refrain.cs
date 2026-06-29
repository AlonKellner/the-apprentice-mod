using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Refrain : ApprenticeCard
{
    public const string CardId = "TheApprentice:Refrain";

    public Refrain() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int cap = IsUpgraded ? 8 : 5;
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveTension(context, creature, cap, cardPlay.Card);
        if (removed > 0)
            await CreatureCmd.GainBlock(creature, removed * 2m, ValueProp.Unpowered, null);
    }
}
