using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class AutoTune : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:AutoTune";

    public AutoTune() : base(3, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPowerNoTip<AutoTunePower>(1);
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<AutoTunePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
