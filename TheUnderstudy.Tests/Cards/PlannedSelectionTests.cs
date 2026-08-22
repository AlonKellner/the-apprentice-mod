using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Planned is order-sensitive, so PlannedSelection always shows the screen (verified in-game). The one pure
// bit: it requires exactly min(desired, eligible) cards — clamped to eligible so the hand UI's confirm
// button (which has no available-count clamp) is always reachable, and 0/nothing-eligible applies nothing.
public class PlannedSelectionTests
{
    [Theory]
    [InlineData(3, 5, 3)]  // more eligible than wanted -> require the full amount
    [InlineData(3, 2, 2)]  // fewer eligible -> require all of them (no soft-lock)
    [InlineData(2, 2, 2)]
    [InlineData(1, 0, 0)]  // nothing eligible -> require nothing (helper applies nothing)
    [InlineData(0, 4, 0)]  // amount 0 (e.g. a 0-stack power) -> nothing
    public void RequiredCount_IsAmountClampedToEligible(int desired, int eligible, int expected)
    {
        Assert.Equal(expected, PlannedSelection.RequiredCount(desired, eligible));
    }
}
