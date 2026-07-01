using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Coda : ApprenticeCard
{
    public const string CardId = "TheApprentice:Coda";

    public Coda() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int cap = IsUpgraded ? 8 : 5;
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveTension(context, creature, cap, cardPlay.Card);
        if (removed <= 0) return;
        // Must be decimal — see Climax.cs for why: passing the bare int `removed` here would pick
        // the (CardModel, Creature?, int hitCount) overload instead of the (..., decimal damage)
        // one, and throw since Coda has no static WithDamage value.
        decimal damage = removed;
        for (int i = 0; i < 2; i++)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, damage).Execute(context);
    }
}
