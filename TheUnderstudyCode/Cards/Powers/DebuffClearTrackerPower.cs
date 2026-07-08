using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Silently auto-attached to the player at combat start (see UnderstudyCard.AfterPlayerTurnStartLate,
// mirroring PlannedCounterPower/InvertTrackerPower's own auto-attach). Unlike
// EmotionalExpression.DebuffCleared (which only fired when this deck's own Invert-conversion code
// explicitly zeroed out one of the 6 invertible debuffs — a real bug for Take Notes, whose own
// description reads as unconditional), this observes AfterPowerAmountChanged directly — the same
// always-firing, global hook InvertTrackerPower uses — so it sees a debuff power hit zero for ANY
// reason: natural per-turn decay, an enemy removing it, a relic, any card at all, and for ANY debuff,
// not just the 6 invertible ones.
public class DebuffClearTrackerPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override List<(string, string)> Localization => new PowerLoc("Debuff Clear Tracker", "", "");

    // A Func rather than a true multicast event, matching EmotionalExpression.DebuffCleared's old
    // shape — needs to be awaited with the live PlayerChoiceContext, and only one subscriber
    // (Take Notes) is expected at a time in practice.
    public static Func<PlayerChoiceContext, Creature, PowerModel, Task>? DebuffCleared;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner) return;
        if (power.Type != PowerType.Debuff) return;
        if (amount >= 0m) return;      // only a decrease can clear a debuff
        if (power.Amount != 0) return; // must have landed exactly on zero, not just reduced
        if (DebuffCleared != null)
            await DebuffCleared(choiceContext, Owner, power);
    }
}
