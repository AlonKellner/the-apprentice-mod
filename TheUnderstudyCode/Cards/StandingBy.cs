using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StandingBy : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StandingBy";

    public StandingBy() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPower<StandingByPower>(1, 1);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.Apply<StandingByPower>(context, creature, this);
        if (cardPlay.Card.IsUpgraded)
        {
            var power = creature.GetPower<StandingByPower>();
            if (power != null) power.ChoiceMode = true;
        }
    }
}
