using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class FinalLessonPowerTests
{
    [Fact]
    public void ComputeCountdown_ThreeTurnsRemaining_DecrementsToTwo_DoesNotDie() =>
        Assert.Equal((2, false), FinalLessonPower.ComputeCountdown(3));

    [Fact]
    public void ComputeCountdown_TwoTurnsRemaining_DecrementsToOne_DoesNotDie() =>
        Assert.Equal((1, false), FinalLessonPower.ComputeCountdown(2));

    [Fact]
    public void ComputeCountdown_OneTurnRemaining_DecrementsToZero_Dies() =>
        Assert.Equal((0, true), FinalLessonPower.ComputeCountdown(1));

    [Fact]
    public void ComputeCountdown_ZeroTurnsRemaining_StaysZero_Dies() =>
        Assert.Equal((0, true), FinalLessonPower.ComputeCountdown(0));
}
