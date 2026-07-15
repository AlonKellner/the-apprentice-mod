using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MustGoOn : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MustGoOn";

    public MustGoOn() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new SelfDebuffVar("Jaded", 2), new EnergyVar(2));
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
        int jaded = (int)DynamicVars["Jaded"].BaseValue;
        await EmotionalExpression.ApplyJadedToSelf(context, player.Creature, jaded, this);
    }
}
