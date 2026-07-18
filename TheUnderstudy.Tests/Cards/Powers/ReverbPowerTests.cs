using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class ReverbPowerTests
{
    [Fact]
    public void ComputeMultiplier_ZeroStacks_ReturnsOne() =>
        Assert.Equal(1m, ReverbPower.ComputeMultiplier(0));

    [Fact]
    public void ComputeMultiplier_OneStack_Doubles() =>
        Assert.Equal(2m, ReverbPower.ComputeMultiplier(1));

    [Fact]
    public void ComputeMultiplier_TwoStacks_Quadruples() =>
        Assert.Equal(4m, ReverbPower.ComputeMultiplier(2));

    [Fact]
    public void ComputeMultiplier_ThreeStacks_Octuples() =>
        Assert.Equal(8m, ReverbPower.ComputeMultiplier(3));
}
