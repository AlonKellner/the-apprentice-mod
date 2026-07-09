using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class CrescendoPowerTests
{
    [Fact]
    public void ComputeMultiplier_ZeroStacks_ReturnsOne() =>
        Assert.Equal(1m, CrescendoPower.ComputeMultiplier(0));

    [Fact]
    public void ComputeMultiplier_OneStack_Doubles() =>
        Assert.Equal(2m, CrescendoPower.ComputeMultiplier(1));

    [Fact]
    public void ComputeMultiplier_TwoStacks_Quadruples() =>
        Assert.Equal(4m, CrescendoPower.ComputeMultiplier(2));

    [Fact]
    public void ComputeMultiplier_ThreeStacks_Octuples() =>
        Assert.Equal(8m, CrescendoPower.ComputeMultiplier(3));
}
