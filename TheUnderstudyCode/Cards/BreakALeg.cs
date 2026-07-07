using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class BreakALeg : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BreakALeg";

    public BreakALeg() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new EnergyVar(1), new IntVar("Vigor", 3));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Vigor"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);

        var creature = player.Creature;
        int vigor = (int)DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, this, false);
    }
}
