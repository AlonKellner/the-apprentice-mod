using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Flat : ApprenticeCard
{
    public const string CardId = "TheApprentice:Flat";

    public Flat() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithTip(typeof(UnvulnerablePower));
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int stacks = IsUpgraded ? 3 : 2;
        await EmotionalExpression.ApplyUnvulnerable(context, creature, stacks, cardPlay.Card);
        await PowerCmd.Apply<StrengthPower>(context, creature, -1m, creature, cardPlay.Card, false);
    }
}
