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
// (1) Feeds EmotionalExpression's "last modified invertible debuff" tracker via the global
//     AfterPowerAmountChanged broadcast — this is what lets an enemy-inflicted (or otherwise
//     externally-caused) Weak/Vulnerable/etc. register as "last modified" for Invert, not just
//     this deck's own Apply/Convert calls.
//
// (2) Bidirectional cancellation for all 6 invertible debuff/buff pairs (Weak/Unweak,
//     Vulnerable/Unvulnerable, Shaken/Unshaken, Limited/Unlimited, Jaded/Unjaded, Frail/Unfrail)
//     via TryModifyPowerAmountReceived/AfterModifyingPowerAmountReceived. This used to live on
//     each Un-X power itself, which required every Un-X power to be pre-attached (even while
//     empty) so it could intercept its own first-ever gain — a power PowerCmd.Apply creates fresh
//     isn't registered as a hook listener until after interception has already run for that same
//     call. Since this tracker is *already* always attached regardless of any pair's current
//     state, it can intercept a gain to a pair that doesn't exist yet just fine — so the Un-X (and
//     X) powers go back to being ordinary powers: created on demand, visible immediately, removed
//     when they decay to nothing, exactly like Strength/Dexterity.
public class InvertTrackerPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override List<(string, string)> Localization => new PowerLoc("Invert Tracker", "", "");

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || amount == 0m) return Task.CompletedTask;
        var debuff = MapToInvertibleDebuff(power);
        if (debuff != null) EmotionalExpression.RecordModified(Owner, debuff.Value);
        return Task.CompletedTask;
    }

    private static InvertibleDebuff? MapToInvertibleDebuff(PowerModel power) => power switch
    {
        WeakPower or UnweakPower => InvertibleDebuff.Weak,
        VulnerablePower or UnvulnerablePower => InvertibleDebuff.Vulnerable,
        ShakenPower or UnshakenPower => InvertibleDebuff.Shaken,
        LimitedPower or UnlimitedPower => InvertibleDebuff.Limited,
        JadedPower or UnjadedPower => InvertibleDebuff.Jaded,
        FrailPower or UnfrailPower => InvertibleDebuff.Frail,
        StrengthPower => InvertibleDebuff.Strength,
        DexterityPower => InvertibleDebuff.Dexterity,
        _ => null
    };

    // Only one interception can be "in flight" at a time (TryModifyPowerAmountReceived always
    // runs to completion, synchronously, before AfterModifyingPowerAmountReceived is called for
    // that same event), so a single pending slot suffices rather than one field per pair.
    private (InvertibleDebuff Debuff, bool ConsumeDebuffSide, int Consumed) _pending;

    // My Own Lesson support ("the roles of invertible buffs and debuffs are reversed"): while
    // Owner has MyOwnLessonPower, an incoming gain to one of the 8 invertible pairs gets fully
    // redirected to its opposite instead of net-cancelled against existing stock. Two separate
    // pending-swap slots because the correction takes a different shape for each case:
    //  - the 6 real pairs redirect to a *different* power type (Weak -> Unweak), so
    //    _pendingPairSwap records which pair and which side to land on;
    //  - Strength/Dexterity have no separate Un-X power — a "debuff" there is just a negative
    //    amount applied to the *same* power (see EmotionalExpression.TransferDebuffsTo's own
    //    precedent), so the correction is simply negating the raw amount, tracked by
    //    _pendingSignFlipSwap.
    // _isSwapping guards against the corrective PowerCmd.Apply below (which re-enters this same
    // method for the power it just landed on) re-triggering the reversed branch again, which would
    // otherwise ping-pong forever.
    private (InvertibleDebuff Debuff, bool ApplyToDebuffSide, int RawAmount)? _pendingPairSwap;
    private (bool IsStrength, int RawAmount)? _pendingSignFlipSwap;
    private bool _isSwapping;

    // canonicalPower is whichever of the 12 powers (6 pairs) is receiving a gain right now. Work
    // out which pair it belongs to and which side is on the receiving end, then reduce the
    // incoming amount by the *opposing* side's current stock, consuming that much of it via
    // AfterModifyingPowerAmountReceived below. ConsumeDebuffSide records which side to decrement:
    // true when the buff just gained (so the debuff side is what gets consumed), false when the
    // debuff just gained (so the buff side is what gets consumed).
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner) return false;

        Invariants.Check(_pending.Consumed <= 0 && _pendingPairSwap == null && _pendingSignFlipSwap == null,
            nameof(InvertTrackerPower) + "." + nameof(TryModifyPowerAmountReceived),
            "a previous interception is still pending when a new one is starting — re-entrancy assumption violated");

        // Strength/Dexterity sign-flip swap must run before the amount > 0 gate below: a "debuff"
        // gain here is a *negative* amount to this same power, not a positive gain to a separate
        // power, so it would otherwise never reach the reversed-mode logic at all.
        if (!_isSwapping && amount != 0m && canonicalPower is StrengthPower or DexterityPower
            && Owner.GetPowerAmount<MyOwnLessonPower>() > 0)
        {
            _pendingSignFlipSwap = (canonicalPower is StrengthPower, (int)amount);
            modifiedAmount = 0m;
            return true;
        }

        if (amount <= 0m) return false;

        if (!_isSwapping && Owner.GetPowerAmount<MyOwnLessonPower>() > 0)
        {
            var pair = EmotionalExpression.IdentifyPair(canonicalPower);
            if (pair != null)
            {
                _pendingPairSwap = (pair.Value.Debuff, !pair.Value.IsDebuffSide, (int)amount);
                modifiedAmount = 0m;
                return true;
            }
        }

        (int reduced, int consumed) result;
        switch (canonicalPower)
        {
            case WeakPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnweakPower>());
                _pending = (InvertibleDebuff.Weak, false, result.consumed);
                break;
            case UnweakPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<WeakPower>());
                _pending = (InvertibleDebuff.Weak, true, result.consumed);
                break;
            case VulnerablePower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnvulnerablePower>());
                _pending = (InvertibleDebuff.Vulnerable, false, result.consumed);
                break;
            case UnvulnerablePower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<VulnerablePower>());
                _pending = (InvertibleDebuff.Vulnerable, true, result.consumed);
                break;
            case ShakenPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnshakenPower>());
                _pending = (InvertibleDebuff.Shaken, false, result.consumed);
                break;
            case UnshakenPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<ShakenPower>());
                _pending = (InvertibleDebuff.Shaken, true, result.consumed);
                break;
            case LimitedPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnlimitedPower>());
                _pending = (InvertibleDebuff.Limited, false, result.consumed);
                break;
            case UnlimitedPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<LimitedPower>());
                _pending = (InvertibleDebuff.Limited, true, result.consumed);
                break;
            case JadedPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnjadedPower>());
                _pending = (InvertibleDebuff.Jaded, false, result.consumed);
                break;
            case UnjadedPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<JadedPower>());
                _pending = (InvertibleDebuff.Jaded, true, result.consumed);
                break;
            case FrailPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<UnfrailPower>());
                _pending = (InvertibleDebuff.Frail, false, result.consumed);
                break;
            case UnfrailPower:
                result = EmotionalExpression.ComputeWeakCancellation((int)amount, Owner.GetPowerAmount<FrailPower>());
                _pending = (InvertibleDebuff.Frail, true, result.consumed);
                break;
            default:
                return false;
        }

        if (result.consumed <= 0) return false;
        modifiedAmount = result.reduced;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (_pendingSignFlipSwap != null)
        {
            var (isStrength, rawAmount) = _pendingSignFlipSwap.Value;
            _pendingSignFlipSwap = null;
            _isSwapping = true;
            try
            {
                var ctx = new ThrowingPlayerChoiceContext();
                if (isStrength)
                    await PowerCmd.Apply<StrengthPower>(ctx, Owner, -rawAmount, Owner, null, false);
                else
                    await PowerCmd.Apply<DexterityPower>(ctx, Owner, -rawAmount, Owner, null, false);
            }
            finally
            {
                _isSwapping = false;
            }
            return;
        }

        if (_pendingPairSwap != null)
        {
            var (debuffPair, applyToDebuffSide, rawAmount) = _pendingPairSwap.Value;
            _pendingPairSwap = null;
            _isSwapping = true;
            try
            {
                var ctx = new ThrowingPlayerChoiceContext();
                if (applyToDebuffSide)
                    await EmotionalExpression.ApplyDebuffSide(ctx, Owner, debuffPair, rawAmount);
                else
                    await EmotionalExpression.ApplyBuffSide(ctx, Owner, debuffPair, rawAmount);
            }
            finally
            {
                _isSwapping = false;
            }
            return;
        }

        if (_pending.Consumed <= 0) return;
        var (debuff, consumeDebuffSide, consumed) = _pending;
        _pending = default;

        var toDecrement = ResolvePower(debuff, consumeDebuffSide);
        if (toDecrement == null)
        {
            Invariants.Check(false, nameof(InvertTrackerPower) + "." + nameof(AfterModifyingPowerAmountReceived),
                $"consumed {consumed} stock of {debuff} ({(consumeDebuffSide ? "debuff" : "buff")} side) via " +
                "interception, but that power is now gone — the two are out of sync.");
            return;
        }
        for (int i = 0; i < consumed; i++)
            await PowerCmd.Decrement(toDecrement);
    }

    private PowerModel? ResolvePower(InvertibleDebuff debuff, bool consumeDebuffSide) => debuff switch
    {
        InvertibleDebuff.Weak => consumeDebuffSide ? Owner.GetPower<WeakPower>() : Owner.GetPower<UnweakPower>(),
        InvertibleDebuff.Vulnerable => consumeDebuffSide ? Owner.GetPower<VulnerablePower>() : Owner.GetPower<UnvulnerablePower>(),
        InvertibleDebuff.Shaken => consumeDebuffSide ? Owner.GetPower<ShakenPower>() : Owner.GetPower<UnshakenPower>(),
        InvertibleDebuff.Limited => consumeDebuffSide ? Owner.GetPower<LimitedPower>() : Owner.GetPower<UnlimitedPower>(),
        InvertibleDebuff.Jaded => consumeDebuffSide ? Owner.GetPower<JadedPower>() : Owner.GetPower<UnjadedPower>(),
        InvertibleDebuff.Frail => consumeDebuffSide ? Owner.GetPower<FrailPower>() : Owner.GetPower<UnfrailPower>(),
        _ => null
    };
}
