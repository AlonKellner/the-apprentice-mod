using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Climax : ApprenticeCard
{
    public const string CardId = "TheApprentice:Climax";

    public Climax() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveAllTension(context, creature, cardPlay.Card);
        if (removed <= 0) return;
        decimal multiplier = IsUpgraded ? 4m : 3m;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, removed * multiplier).Execute(context);
    }
}
