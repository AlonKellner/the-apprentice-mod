using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class LimitedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Limited",
        "Draw 1 fewer card at the start of your next turn.",
        "Draw 1 fewer card at the start of your next turn.");

    private bool _appliedThisDraw;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player) return count;
        _appliedThisDraw = true;
        return Math.Max(0m, count - 1m);
    }

    public override async Task AfterModifyingHandDraw()
    {
        if (!_appliedThisDraw) return;
        _appliedThisDraw = false;
        Flash();
        await PowerCmd.Decrement(this);
    }
}
