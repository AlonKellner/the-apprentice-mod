using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class StageManagerPowerTests
{
    [Fact]
    public void ShouldAutoPlayPlanned_NoCardsPlayed_ReturnsTrue() =>
        Assert.True(IntermissionPower.ShouldAutoPlayPlanned(0));

    [Fact]
    public void ShouldAutoPlayPlanned_OneCardPlayed_ReturnsFalse() =>
        Assert.False(IntermissionPower.ShouldAutoPlayPlanned(1));

    [Fact]
    public void ShouldAutoPlayPlanned_SeveralCardsPlayed_ReturnsFalse() =>
        Assert.False(IntermissionPower.ShouldAutoPlayPlanned(3));
}
