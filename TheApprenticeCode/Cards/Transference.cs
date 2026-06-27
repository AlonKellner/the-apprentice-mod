using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Transference : ApprenticeCard
{
    public const string CardId = "TheApprentice:Transference";

    public Transference() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        var target = cardPlay.Target!;
        await EmotionalExpression.TransferWeakTo(context, creature, target, cardPlay.Card);
        if (IsUpgraded)
            await EmotionalExpression.TransferVulnerableTo(context, creature, target, cardPlay.Card);
    }
}
