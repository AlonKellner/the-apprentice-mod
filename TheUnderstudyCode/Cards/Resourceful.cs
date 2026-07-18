using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Turn a jam into fuel: the first Unplayable in hand each turn draws you fresh cards.
public class Resourceful : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Resourceful";

    public Resourceful() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<ResourcefulPower>(2, 1);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<ResourcefulPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
