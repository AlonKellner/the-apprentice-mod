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
using MegaCrit.Sts2.Core.Logging;
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
        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        bool heldNote = HeldNotePower.IsActive(Owner);
        Log.Info($"[LessonDiag] LimitedPower.AfterSideTurnStart[turn {turn}, side {side}]: Amount={Amount}, heldNote={heldNote} -> {(heldNote ? "no decrement" : "decrement -1")}");
        if (!heldNote)
        {
            Invariants.Check(Amount > 0, nameof(LimitedPower) + "." + nameof(AfterSideTurnStart),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
