using MegaCrit.Sts2.Core.Entities.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class PulledPunchPowerTests
{
    [Fact]
    public void ComputeDebuffPairSteps_FreshThreeWeak_ReducesByOne() =>
        Assert.Equal(1, PulledPunchPower.ComputeDebuffPairSteps(PowerType.Debuff, 3m, 3, 1));

    [Fact]
    public void ComputeDebuffPairSteps_FreshOneLimited_UpgradedReduceByTwo_CapsAtOne() =>
        Assert.Equal(1, PulledPunchPower.ComputeDebuffPairSteps(PowerType.Debuff, 1m, 1, 2));

    [Fact]
    public void ComputeDebuffPairSteps_DecayTickDecrement_DoesNotReact() =>
        Assert.Equal(0, PulledPunchPower.ComputeDebuffPairSteps(PowerType.Debuff, -1m, 2, 1));

    [Fact]
    public void ComputeDebuffPairSteps_ZeroAmount_DoesNotReact() =>
        Assert.Equal(0, PulledPunchPower.ComputeDebuffPairSteps(PowerType.Debuff, 0m, 2, 1));

    [Fact]
    public void ComputeDebuffPairSteps_BuffTypedSide_DoesNotReact() =>
        Assert.Equal(0, PulledPunchPower.ComputeDebuffPairSteps(PowerType.Buff, 3m, 3, 1));

    [Fact]
    public void ComputeSignFlipSteps_NegativeTwoDexterity_ReducesByOne() =>
        Assert.Equal(1, PulledPunchPower.ComputeSignFlipSteps(-2m, -2, 1));

    [Fact]
    public void ComputeSignFlipSteps_OvershootCapped_DoesNotFlipToPositive() =>
        Assert.Equal(1, PulledPunchPower.ComputeSignFlipSteps(-3m, -1, 2));

    [Fact]
    public void ComputeSignFlipSteps_PositiveGain_DoesNotReact() =>
        Assert.Equal(0, PulledPunchPower.ComputeSignFlipSteps(2m, 5, 1));

    [Fact]
    public void ComputeSignFlipSteps_DebuffHitDidNotCrossZero_DoesNotReact() =>
        Assert.Equal(0, PulledPunchPower.ComputeSignFlipSteps(-2m, 3, 1));
}
