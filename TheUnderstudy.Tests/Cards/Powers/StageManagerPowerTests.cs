using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class StageManagerPowerTests
{
    [Fact]
    public void ShouldAutoPlayPlanned_NoCardsPlayed_ReturnsTrue() =>
        Assert.True(VenuePower.ShouldAutoPlayPlanned(0));

    [Fact]
    public void ShouldAutoPlayPlanned_OneCardPlayed_ReturnsFalse() =>
        Assert.False(VenuePower.ShouldAutoPlayPlanned(1));

    [Fact]
    public void ShouldAutoPlayPlanned_SeveralCardsPlayed_ReturnsFalse() =>
        Assert.False(VenuePower.ShouldAutoPlayPlanned(3));
}
