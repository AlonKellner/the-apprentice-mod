using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Burn the candle for energy now, jaded later. (Jaded self-debuff downside; the deck's Common
// energy on-ramp.)
public class BurnOut : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BurnOut";

    public BurnOut() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new EnergyVar(2));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithTip(typeof(JadedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
        await EmotionalExpression.ApplyJadedToSelf(context, player.Creature, 2, this);
    }
}
