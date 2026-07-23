using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Silently auto-attached to the player at combat start (see UnderstudyCard.AfterPlayerTurnStartLate,
// mirroring PlannedCounterPower's own auto-attach). Two jobs, both relying on being an
// always-attached, always-hidden observer of every power change on the player:
//
// (1) Bidirectional cancellation for all 6 invertible debuff/buff pairs (Weak/Unweak,
//     Vulnerable/Unvulnerable, Shaken/Unshaken, Limited/Unlimited, Jaded/Unjaded, Frail/Unfrail)
//     via TryModifyPowerAmountReceived/AfterModifyingPowerAmountReceived. This used to live on
//     each Un-X power itself, which required every Un-X power to be pre-attached (even while
//     empty) so it could intercept its own first-ever gain — a power PowerCmd.Apply creates fresh
//     isn't registered as a hook listener until after interception has already run for that same
//     call. Since this tracker is *already* always attached regardless of any pair's current
//     state, it can intercept a gain to a pair that doesn't exist yet just fine — so the Un-X (and
//     X) powers go back to being ordinary powers: created on demand, visible immediately, removed
//     when they decay to nothing, exactly like Strength/Dexterity.
//
// (2) Pulled Punch dampening: while the player holds ApathyPower, every incoming invertible
//     debuff is softened toward 0 by that power's Amount, inside the same single interception used
//     for (1). Kept here — rather than as a separate TryModifyPowerAmountReceived listener on
//     ApathyPower — so there is only ever one interceptor for the event and no order-race
//     against the cancellation (the exact hazard MyOwnLessonPower documents avoiding).
public class InvertTrackerPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;


    // Only one interception can be "in flight" at a time (TryModifyPowerAmountReceived always runs to
    // completion, synchronously, before AfterModifyingPowerAmountReceived is called for that same event),
    // so a single pending slot suffices. Records which pair and which side just gained, plus how much of
    // the opposing side to consume in AfterModifyingPowerAmountReceived.
    private (InvertiblePair? Pair, PowerModel? GainedSide, int Consumed) _pending;

    // My Own Lesson support ("the roles of invertible buffs and debuffs are reversed"): while Owner holds
    // MyOwnLessonPower, an incoming DEBUFF-side gain is fully redirected to the pair's BUFF side instead of
    // net-cancelled; a buff-side gain becomes nothing. One unified slot recording the pair + how many buff
    // stacks to apply (for sign-flip powers a negative "debuff" amount becomes a positive buff count).
    // _isSwapping guards the corrective PowerCmd.Apply below (which re-enters this method for the power it
    // just landed on) from re-triggering the reversed branch and ping-ponging forever.
    private (InvertiblePair Pair, int BuffStacks)? _pendingSwap;
    private bool _isSwapping;

    // canonicalPower is whichever invertible power is receiving a gain now. Find its pair (InvertiblePairs),
    // then either redirect (My Own Lesson), soften (Pulled Punch), or net the incoming amount against the
    // opposing side's stock — consuming that much of it via AfterModifyingPowerAmountReceived below.
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner) return false;

        Invariants.Check(_pending.Consumed <= 0 && _pendingSwap == null,
            nameof(InvertTrackerPower) + "." + nameof(TryModifyPowerAmountReceived),
            "a previous interception is still pending when a new one is starting — re-entrancy assumption violated");

        var pair = InvertiblePairs.For(canonicalPower);
        if (pair == null) return false;

        // Pulled Punch (job 3): while Owner holds ApathyPower, every incoming invertible debuff is softened
        // toward 0. Done here rather than as its own listener so there's one interceptor and no order-race
        // with the cancellation below (see MyOwnLessonPower for that reasoning).
        int pulledPunch = Owner.GetPowerAmount<ApathyPower>();
        bool myOwnLesson = !_isSwapping && Owner.GetPowerAmount<MyOwnLessonPower>() > 0;

        // Sign-flip pairs (Strength/Dexterity/Vigor): a "debuff" is a NEGATIVE amount to the same power, so
        // it must be handled before the amount > 0 gate. They self-net (no opposing stock), so outside of
        // My Own Lesson / Pulled Punch there is nothing to intercept.
        if (!pair.IsSameShape)
        {
            if (myOwnLesson && amount != 0m)
            {
                modifiedAmount = 0m;
                // Debuff (negative) gain flips to a buff of that magnitude; a buff (positive) gain becomes nothing.
                if (amount < 0m) _pendingSwap = (pair, -(int)amount);
                return true;
            }
            if (pulledPunch > 0 && amount < 0m)
            {
                modifiedAmount = ApathyPower.Dampen(amount, isSignFlip: true, pulledPunch);
                return true;
            }
            return false;
        }

        if (amount <= 0m) return false;   // same-shape removals aren't intercepted

        bool gainedDebuffSide = pair.IsDebuffSide(canonicalPower);

        if (myOwnLesson)
        {
            modifiedAmount = 0m;
            // Debuff-side gain becomes the buff side; buff-side gain becomes nothing.
            if (gainedDebuffSide) _pendingSwap = (pair, (int)amount);
            return true;
        }

        // Pulled Punch softens the incoming debuff-side gain toward 0 BEFORE it cancels against opposing buff
        // stock, so a hit Pulled Punch fully absorbs doesn't also waste your buffs. Only the debuff side.
        decimal originalAmount = amount;
        if (pulledPunch > 0 && gainedDebuffSide)
            amount = ApathyPower.Dampen(amount, isSignFlip: false, pulledPunch);

        var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, pair.OpposingStock(Owner, canonicalPower));
        if (consumed <= 0)
        {
            // No opposing stock to cancel against, but Pulled Punch may still have softened the hit.
            if (amount != originalAmount) { modifiedAmount = amount; return true; }
            return false;
        }
        _pending = (pair, canonicalPower, consumed);
        modifiedAmount = reduced;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (_pendingSwap != null)
        {
            var (pair, buffStacks) = _pendingSwap.Value;
            _pendingSwap = null;
            _isSwapping = true;
            try
            {
                await pair.ApplyBuffSide(new ThrowingPlayerChoiceContext(), Owner, buffStacks);
            }
            finally
            {
                _isSwapping = false;
            }
            return;
        }

        if (_pending.Consumed <= 0) return;
        var (pending, gainedSide, consumed) = _pending;
        _pending = default;
        await pending!.DecrementOpposing(Owner, gainedSide!, consumed);
    }
}
