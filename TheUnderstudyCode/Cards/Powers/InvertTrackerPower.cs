using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

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
        if (target != Owner || amount <= 0m) return false;

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
        if (_pending.Consumed <= 0) return;
        var (debuff, consumeDebuffSide, consumed) = _pending;
        _pending = default;

        var toDecrement = ResolvePower(debuff, consumeDebuffSide);
        if (toDecrement == null)
        {
            Log.Error($"InvertTrackerPower consumed {consumed} stock of {debuff} " +
                      $"({(consumeDebuffSide ? "debuff" : "buff")} side) via interception, but that " +
                      "power is now gone — the two are out of sync.");
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
