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

    // Unweak/Unvulnerable are mod-owned, so they skip their own decay directly (see
    // UnweakPower/UnvulnerablePower.AfterSideTurnEnd) when this power is present — no snapshot
    // needed for those two. Weak/Vulnerable are base-game powers whose decay this mod can't gate
    // at the source, so they're still snapshotted here and restored next turn.
    private decimal _weakStored;
    private decimal _vulnStored;

    // Snapshot EE state at end of player's turn, before enemy acts and debuffs tick down
    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;
        _weakStored = Owner.GetPowerAmount<WeakPower>();
        _vulnStored = Owner.GetPowerAmount<VulnerablePower>();
        return Task.CompletedTask;
    }

    // Restore any Weak/Vulnerable that ticked down during the enemy's turn. Applying these
    // positively still goes through the normal reactive cancellation against Unweak/Unvulnerable
    // (see UnweakPower/UnvulnerablePower.TryModifyPowerAmountReceived), so this can't create a
    // coexistence bug the way restoring Unweak/Unvulnerable directly would.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;

        decimal weakNow = Owner.GetPowerAmount<WeakPower>();
        if (_weakStored > weakNow)
            await PowerCmd.Apply<WeakPower>(context, Owner, _weakStored - weakNow, Owner, null, false);

        decimal vulnNow = Owner.GetPowerAmount<VulnerablePower>();
        if (_vulnStored > vulnNow)
            await PowerCmd.Apply<VulnerablePower>(context, Owner, _vulnStored - vulnNow, Owner, null, false);
    }
}
