using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Enjoy the Ride's one-shot reactive Invert: the very next time an invertible debuff is applied to the
// owner, Invert Amount of that specific debuff, then remove itself so it never fires again. Amount is
// the Invert value (2).
public class EnjoyTheRidePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Enjoy the Ride",
        "[gold]Invert[/gold] this many of the next invertible debuff to be modified.",
        "[gold]Invert[/gold] [blue]{Amount}[/blue] of the next invertible debuff to be modified.");

    // Guards against re-entrancy only: InvertDebuff below changes power amounts, which re-enters this
    // hook synchronously before the removal command has processed. Durability isn't needed — the power
    // is removed on this same trigger, so there is no future modification for it to react to.
    private bool _fired;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_fired || power.Owner != Owner || amount <= 0m) return;
        var id = EmotionalExpression.IdentifyPair(power);
        if (id == null || !id.Value.IsDebuffSide) return;

        _fired = true;
        await EmotionalExpression.InvertDebuff(choiceContext, Owner, id.Value.Debuff, (int)Amount);
        // One-time use: gone after this single inversion (InvertDebuff already mutates powers in this
        // same hook, so removing here is the same established pattern).
        await PowerCmd.Remove(this);
    }
}
