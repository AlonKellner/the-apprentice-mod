using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class FermataPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Fermata",
        "[gold]Weak[/gold], [gold]Vulnerable[/gold], [gold]Unweak[/gold], and [gold]Unvulnerable[/gold] no longer decrease at the end of your turn.",
        "[gold]Weak[/gold], [gold]Vulnerable[/gold], [gold]Unweak[/gold], and [gold]Unvulnerable[/gold] no longer decrease at the end of your turn.");

    private decimal _weakStored;
    private decimal _unweakStored;
    private decimal _vulnStored;
    private decimal _unvulnStored;

    // Snapshot EE state at end of player's turn, before enemy acts and debuffs tick down
    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;
        _weakStored   = Owner.GetPowerAmount<WeakPower>();
        _unweakStored = Owner.GetPowerAmount<UnweakPower>();
        _vulnStored   = Owner.GetPowerAmount<VulnerablePower>();
        _unvulnStored = Owner.GetPowerAmount<UnvulnerablePower>();
        return Task.CompletedTask;
    }

    // Restore any stacks that ticked down during the enemy's turn
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;

        decimal weakNow = Owner.GetPowerAmount<WeakPower>();
        if (_weakStored > weakNow)
            await PowerCmd.Apply<WeakPower>(context, Owner, _weakStored - weakNow, Owner, null, false);

        decimal unweakNow = Owner.GetPowerAmount<UnweakPower>();
        if (_unweakStored > unweakNow)
            await PowerCmd.Apply<UnweakPower>(context, Owner, _unweakStored - unweakNow, Owner, null, false);

        decimal vulnNow = Owner.GetPowerAmount<VulnerablePower>();
        if (_vulnStored > vulnNow)
            await PowerCmd.Apply<VulnerablePower>(context, Owner, _vulnStored - vulnNow, Owner, null, false);

        decimal unvulnNow = Owner.GetPowerAmount<UnvulnerablePower>();
        if (_unvulnStored > unvulnNow)
            await PowerCmd.Apply<UnvulnerablePower>(context, Owner, _unvulnStored - unvulnNow, Owner, null, false);
    }
}
