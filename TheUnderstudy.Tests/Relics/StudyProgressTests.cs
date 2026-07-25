using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

// Pure arithmetic for the Chaotic Book's Study quest. The relic wrapper (Log.*/RelicCmd) is verified
// live in-game; this covers the counting itself, which must be exact for the transform to fire.
public class StudyProgressTests
{
    [Theory]
    [InlineData(0, 3, 3)]
    [InlineData(1, 3, 2)]
    [InlineData(2, 3, 1)]
    [InlineData(3, 3, 0)]
    [InlineData(4, 3, 0)] // never negative, even if over-counted
    public void Remaining_CountsDownToZeroAndClamps(int studied, int required, int expected) =>
        Assert.Equal(expected, StudyProgress.Remaining(studied, required));

    [Theory]
    [InlineData(0, 3, false)]
    [InlineData(2, 3, false)]
    [InlineData(3, 3, true)]  // transforms exactly at the threshold
    [InlineData(5, 3, true)]
    public void IsComplete_TrueAtOrAboveThreshold(int studied, int required, bool expected) =>
        Assert.Equal(expected, StudyProgress.IsComplete(studied, required));
}
