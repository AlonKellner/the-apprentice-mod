using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Unburdening : ApprenticeCard
{
    public const string CardId = "TheApprentice:Unburdening";

    public Unburdening() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(UnvulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int stacks = IsUpgraded ? 2 : 1;
        await EmotionalExpression.ApplyUnvulnerable(context, creature, stacks, cardPlay.Card);
    }
}
