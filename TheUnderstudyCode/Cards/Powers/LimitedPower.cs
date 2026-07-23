using System;
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
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class LimitedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player) return count;
        return Math.Max(0m, count - 1m);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        if (!HeldNotePower.IsActive(Owner))
        {
            Invariants.Check(Amount > 0, nameof(LimitedPower) + "." + nameof(AfterSideTurnStart),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
