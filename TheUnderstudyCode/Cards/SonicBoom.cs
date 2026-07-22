using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class SonicBoom : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SonicBoom";

    protected override bool HasEnergyCostX => true;

    public SonicBoom() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Vigor", 4));
        WithMarkedTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Vigor"].UpgradeValueBy(2m); // 4 -> 6 per X
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        int perStack = (int)DynamicVars["Vigor"].BaseValue;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, perStack * x, creature, this, false);
    }
}
