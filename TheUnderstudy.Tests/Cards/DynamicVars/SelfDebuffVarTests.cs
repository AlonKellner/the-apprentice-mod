using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using Xunit;

namespace TheUnderstudy.Tests.Cards.DynamicVars;

public class SelfDebuffVarTests
{
    [Fact]
    public void ComputePreview_NoPulledPunch_ReturnsBaseUnchanged() =>
        Assert.Equal(2m, SelfDebuffVar.ComputePreview(2m, pulledPunch: 0));

    [Fact]
    public void ComputePreview_OnePulledPunch_ReducesByOne() =>
        Assert.Equal(1m, SelfDebuffVar.ComputePreview(2m, pulledPunch: 1));

    [Fact]
    public void ComputePreview_TwoPulledPunch_ReducesToZero() =>
        Assert.Equal(0m, SelfDebuffVar.ComputePreview(2m, pulledPunch: 2));

    [Fact]
    public void ComputePreview_PulledPunchExceedsBase_ClampsAtZero_NeverNegative() =>
        Assert.Equal(0m, SelfDebuffVar.ComputePreview(1m, pulledPunch: 3));
}
