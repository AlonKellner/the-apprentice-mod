using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Defiance : ApprenticeCard
{
    public const string CardId = "TheApprentice:Defiance";

    public Defiance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(UnvulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int stacks = IsUpgraded ? 3 : 2;
        await EmotionalExpression.ApplyUnvulnerable(context, creature, stacks, cardPlay.Card);
    }
}
