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
        await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, removed * 2m).Execute(context);
    }
}
