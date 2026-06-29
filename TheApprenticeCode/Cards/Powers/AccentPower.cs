using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class AccentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Accent",
        "The next [gold]Weak[/gold], [gold]Unweak[/gold], [gold]Vulnerable[/gold], or [gold]Unvulnerable[/gold] applied to you is doubled.",
        "The next [gold]Weak[/gold], [gold]Unweak[/gold], [gold]Vulnerable[/gold], or [gold]Unvulnerable[/gold] applied to you is doubled.");

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;
        if (canonicalPower is not (WeakPower or UnweakPower or VulnerablePower or UnvulnerablePower)) return false;
        modifiedAmount = amount * 2m;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        await PowerCmd.Decrement(this);
    }
}
