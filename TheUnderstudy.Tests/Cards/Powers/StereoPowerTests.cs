using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class StereoPowerTests
{
    [Fact]
    public void ComputeMultiplier_ZeroStacks_ReturnsOne() =>
        Assert.Equal(1m, StereoPower.ComputeMultiplier(0));

    [Fact]
    public void ComputeMultiplier_OneStack_Doubles() =>
        Assert.Equal(2m, StereoPower.ComputeMultiplier(1));

    [Fact]
    public void ComputeMultiplier_TwoStacks_Quadruples() =>
        Assert.Equal(4m, StereoPower.ComputeMultiplier(2));

    [Fact]
    public void ComputeMultiplier_ThreeStacks_Octuples() =>
        Assert.Equal(8m, StereoPower.ComputeMultiplier(3));
}
