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
        // Must be decimal: CommonActions.CardAttack(CardModel, Creature?, int) is a DIFFERENT
        // overload (hitCount against the card's own static WithDamage value, which Climax doesn't
        // have) from CardAttack(CardModel, Creature?, decimal) (an explicit damage amount, which
        // is what's needed here). Passing the bare int `removed` picks the former and throws
        // "does not have a damage variable supported by CommonActions.CardAttack".
        decimal damage = removed;
        int hits = IsUpgraded ? 4 : 3;
        for (int i = 0; i < hits; i++)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, damage).Execute(context);
    }
}
