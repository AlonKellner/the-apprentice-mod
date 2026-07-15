using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Enjoy the Ride's one-shot reactive Invert: the next time an invertible debuff is applied to the
// owner, Invert Amount of that specific debuff, then go dormant. Amount is the Invert value (2).
public class EnjoyTheRidePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Enjoy the Ride",
        "The next time an invertible debuff is modified, [gold]Invert[/gold] this many of that debuff.",
        "The next time an invertible debuff is modified, [gold]Invert[/gold] [blue]{Amount}[/blue] of that debuff.");

    private bool _fired;

    // Fires when a debuff side of an invertible pair is applied/increased on the owner. Inverting the
    // debuff below re-enters this hook (it changes power amounts), so _fired guards against recursion.
    // We do NOT remove the power here — removing a power mid-broadcast has no precedent in this mod; the
    // safe AfterPlayerTurnStart hook cleans it up once it has fired.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_fired || power.Owner != Owner || amount <= 0m) return;
        var id = EmotionalExpression.IdentifyPair(power);
        if (id == null || !id.Value.IsDebuffSide) return;

        _fired = true;
        await EmotionalExpression.InvertDebuff(choiceContext, Owner, id.Value.Debuff, (int)Amount);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (_fired && player == Owner.Player)
            await PowerCmd.Remove(this);
    }
}
