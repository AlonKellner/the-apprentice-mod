using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Reverb : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Reverb";

    public Reverb() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Vigor", 3));
        WithPowerNoTip<ReverbPower>(1);
        WithMarkedTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Vigor"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, (int)DynamicVars["Vigor"].BaseValue, creature, this, false);
        await CommonActions.Apply<ReverbPower>(context, creature, this);
    }
}
