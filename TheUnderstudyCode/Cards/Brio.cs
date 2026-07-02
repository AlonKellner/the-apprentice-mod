using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Brio : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Brio";

    public Brio() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<BrioPower>(context, creature, IsUpgraded ? 5m : 3m, creature, cardPlay.Card, false);
    }
}
