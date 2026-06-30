using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Transcendence : ApprenticeCard
{
    public const string CardId = "TheApprentice:Transcendence";

    public Transcendence() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithTip(typeof(UnweakPower));
        WithTip(typeof(UnvulnerablePower));
    }

    public override bool HasExpend => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int stacks = IsUpgraded ? 2 : 1;
        await EmotionalExpression.ApplyUnweak(context, creature, stacks, cardPlay.Card);
        await EmotionalExpression.ApplyUnvulnerable(context, creature, stacks, cardPlay.Card);
    }
}
