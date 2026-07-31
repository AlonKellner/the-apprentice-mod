using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Powers;

// Crying Out Loud grants Vigor whenever a debuff of yours clears. Negative Vigor is a functional debuff
// (Buff-typed but AllowNegative), and it clears when it rises from below 0 back to zero-or-positive —
// spending it on an attack zeroes it, Invert/Swap/Silence can lift it. The pure decision the reactive
// AfterPowerAmountChanged hook uses is covered here (the hook wiring itself needs a live combat, per the
// no-combat-harness note).
public class CryingOutLoudPowerTests
{
    [Theory]
    // was negative, now >= 0 -> a debuff cleared
    [InlineData(-3, 0, true)]    // spent on an attack: -3 -> 0
    [InlineData(-3, 2, true)]    // lifted past zero: -3 -> +2
    [InlineData(-1, 0, true)]
    // still negative -> not yet cleared
    [InlineData(-5, -2, false)]  // partial reduction only
    [InlineData(-3, -3, false)]
    // never negative -> not a debuff clearing (gaining/spending positive Vigor must not grant)
    [InlineData(0, 3, false)]    // gaining Vigor from 0
    [InlineData(3, 0, false)]    // spending positive Vigor down to 0
    [InlineData(5, 2, false)]
    public void ClearedNegativeVigor(int oldAmount, int newAmount, bool expected) =>
        Assert.Equal(expected, CryingOutLoudPower.ClearedNegativeVigor(oldAmount, newAmount));
}
