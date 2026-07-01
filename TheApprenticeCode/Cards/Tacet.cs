using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tacet : ApprenticeCardB
{
    public const string CardId = "TheApprentice:Tacet";

    public Tacet() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<TacetPower>(context, creature, IsUpgraded ? 3m : 2m, creature, cardPlay.Card, false);
    }
}
