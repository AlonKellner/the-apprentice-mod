using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// A plain counter power: its only job is to record "reduce incoming invertible debuffs by Amount".
// The actual reduction is applied by InvertTrackerPower, which already owns the single
// TryModifyPowerAmountReceived interception for every invertible pair — folding the dampening in
// there (gated on Owner.GetPowerAmount<ApathyPower>()) keeps one deterministic interceptor
// rather than racing a second one (see MyOwnLessonPower for why a separate interceptor is avoided).
public class ApathyPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    // Pure reduction of a single invertible-debuff application toward zero, by reduceBy, clamped so
    // it never overshoots past 0 into the opposite sign. Two shapes:
    //  - normal debuff powers (Weak/Vulnerable/Shaken/Limited/Jaded/Frail) gain a POSITIVE amount,
    //    so reduce means subtract toward 0;
    //  - sign-flip powers (Strength/Dexterity) express a debuff as a NEGATIVE amount to the same
    //    power, so reduce means add toward 0.
    public static decimal Dampen(decimal amount, bool isSignFlip, int reduceBy) =>
        isSignFlip ? Math.Min(0m, amount + reduceBy) : Math.Max(0m, amount - reduceBy);
}
