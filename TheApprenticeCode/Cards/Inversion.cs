using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Inversion : ApprenticeCard
{
    public const string CardId = "TheApprentice:Inversion";

    public Inversion() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithCostUpgradeBy(-1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(UnweakPower));
        WithTip(typeof(UnvulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ConvertWeakToUnweak(context, creature, 5);
        await EmotionalExpression.ConvertVulnerableToUnvulnerable(context, creature, 5);
    }
}
