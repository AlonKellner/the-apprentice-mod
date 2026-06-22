using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

public class PlannedTrackerTests
{
    [Fact]
    public void InitialSequenceIsZero()
    {
        PlannedTracker.ResetSequence();
        Assert.Equal(0, PlannedTracker.CurrentSequence);
    }

    [Fact]
    public void ResetSetsToZero()
    {
        PlannedTracker.CurrentSequence = 5;
        PlannedTracker.ResetSequence();
        Assert.Equal(0, PlannedTracker.CurrentSequence);
    }

    [Fact]
    public void SequenceIncreasesOnIncrement()
    {
        PlannedTracker.ResetSequence();
        PlannedTracker.CurrentSequence++;
        PlannedTracker.CurrentSequence++;
        Assert.Equal(2, PlannedTracker.CurrentSequence);
    }

    [Fact]
    public void AfterReset_NewSequenceStartsFromZero()
    {
        PlannedTracker.CurrentSequence = 99;
        PlannedTracker.ResetSequence();
        var captured = PlannedTracker.CurrentSequence++;
        Assert.Equal(0, captured);
        Assert.Equal(1, PlannedTracker.CurrentSequence);
    }
}
