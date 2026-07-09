using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Showstopper : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Showstopper";

    public Showstopper() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // Starts Intense 1 — converts from "always replayable" to "one big hit, then needs
        // freeing," so the damage is raised to compensate. The energy refund stays: "the show
        // must go on" reads fine even on a one-off.
        WithDamage(34);
        WithVars(new EnergyVar(1));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithTip(UnderstudyKeywords.Intense);
    }

    public override bool IsPreIntense => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(8m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
    }
}
