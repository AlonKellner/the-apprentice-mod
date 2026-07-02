using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Benched : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Benched";

    public Benched() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
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
        await PowerCmd.Apply<BenchedPower>(context, creature, IsUpgraded ? 3m : 2m, creature, cardPlay.Card, false);
    }
}
