using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Vibrato : ApprenticeCard
{
    public const string CardId = "TheApprentice:Vibrato";

    public Vibrato() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int tension = TensionHelper.GetTension(creature);
        if (tension <= 0) return;
        // Tension is NOT removed.
        // Must be decimal — see Climax.cs for why: passing the bare int `tension` here would pick
        // the (CardModel, Creature?, int hitCount) overload instead of the (..., decimal damage)
        // one, and throw since Vibrato has no static WithDamage value.
        decimal damage = tension;
        int hits = IsUpgraded ? 2 : 1;
        for (int i = 0; i < hits; i++)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, damage).Execute(context);
    }
}
