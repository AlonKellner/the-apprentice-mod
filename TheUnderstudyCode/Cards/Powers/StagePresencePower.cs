using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Recurring passive Swap: every turn start, trade fortunes with the enemy team. Amount is the per-turn
// Swap repeat count and now STACKS (Counter) — N copies Swap N times per turn.
//
// Living the Dream: when Bright Side is ALSO present, this power runs a single simultaneous Best of Both
// (Swap this Amount & Invert Bright Side's Amount, at once) instead of a separate Swap, and Bright Side
// skips its separate Invert — so the two combine rather than resolving independently. Hosting the combined
// effect here (not on the marker) keeps it correct regardless of hook order and marker-sync timing, since
// this power is always present exactly when the pair is.
public class StagePresencePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        int bright = Owner.GetPowerAmount<BrightSidePower>();
        if (bright > 0)
            await BestOfBoth.ResolveFor(context, Owner, (int)Amount, bright); // Living the Dream: simultaneous
        else
            await SceneStealing.Swap(context, Owner, (int)Amount);
    }

    // Keep the visible Living the Dream marker in sync whenever any power on the owner changes.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner == Owner) await LivingTheDreamPower.Sync(context, Owner);
    }
}
