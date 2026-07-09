using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class PulledPunchPowerTests
{
    // ── Normal debuff powers (positive gain, reduce toward 0) ────────────────────────────────────

    [Fact]
    public void Dampen_FreshThreeWeak_ReducedByOne() =>
        Assert.Equal(2m, PulledPunchPower.Dampen(3m, isSignFlip: false, reduceBy: 1));

    [Fact]
    public void Dampen_OneLimited_UpgradedReduceByTwo_ClampsAtZero() =>
        Assert.Equal(0m, PulledPunchPower.Dampen(1m, isSignFlip: false, reduceBy: 2));

    [Fact]
    public void Dampen_OneWeak_ReduceByOne_LandsExactlyZero() =>
        Assert.Equal(0m, PulledPunchPower.Dampen(1m, isSignFlip: false, reduceBy: 1));

    // ── Sign-flip powers (Strength/Dexterity debuff is a negative amount, reduce toward 0) ───────

    [Fact]
    public void Dampen_NegativeTwoDexterity_ReducedByOne() =>
        Assert.Equal(-1m, PulledPunchPower.Dampen(-2m, isSignFlip: true, reduceBy: 1));

    [Fact]
    public void Dampen_NegativeOne_UpgradedReduceByTwo_ClampsAtZero_DoesNotFlipPositive() =>
        Assert.Equal(0m, PulledPunchPower.Dampen(-1m, isSignFlip: true, reduceBy: 2));

    [Fact]
    public void Dampen_NegativeTwo_ReduceByTwo_LandsExactlyZero() =>
        Assert.Equal(0m, PulledPunchPower.Dampen(-2m, isSignFlip: true, reduceBy: 2));
}
