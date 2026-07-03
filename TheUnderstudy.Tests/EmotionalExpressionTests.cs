using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

public class EmotionalExpressionTests
{
    // CountUniqueDebuffTypes

    [Fact]
    public void CountUniqueDebuffTypes_NonePresent_Returns0() =>
        Assert.Equal(0, EmotionalExpression.CountUniqueDebuffTypes(0, 0));

    [Fact]
    public void CountUniqueDebuffTypes_OnlyWeak_Returns1() =>
        Assert.Equal(1, EmotionalExpression.CountUniqueDebuffTypes(3, 0));

    [Fact]
    public void CountUniqueDebuffTypes_OnlyVulnerable_Returns1() =>
        Assert.Equal(1, EmotionalExpression.CountUniqueDebuffTypes(0, 5));

    [Fact]
    public void CountUniqueDebuffTypes_BothPresent_Returns2() =>
        Assert.Equal(2, EmotionalExpression.CountUniqueDebuffTypes(2, 4));

    // ComputeWeakCancellation — the one shared cancellation primitive every Un-X power's
    // bidirectional TryModifyPowerAmountReceived now leans on (both directions, all 6 pairs), so
    // its edge cases matter more than before this redesign.

    [Fact]
    public void ComputeWeakCancellation_NoneAvailable_PassesThrough() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeWeakCancellation(3, 0));

    [Fact]
    public void ComputeWeakCancellation_ExactMatch_BlocksAll() =>
        Assert.Equal((0, 3), EmotionalExpression.ComputeWeakCancellation(3, 3));

    [Fact]
    public void ComputeWeakCancellation_AppliedExceedsAvailable_ReducesPartially() =>
        Assert.Equal((1, 2), EmotionalExpression.ComputeWeakCancellation(3, 2));

    [Fact]
    public void ComputeWeakCancellation_AvailableExceedsApplied_BlocksAll() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeWeakCancellation(2, 5));

    [Fact]
    public void ComputeWeakCancellation_BothZero_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeWeakCancellation(0, 0));

    [Fact]
    public void ComputeWeakCancellation_ZeroApplied_NoOpRegardlessOfAvailable() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeWeakCancellation(0, 4));

    // ComputeSignFlip — Strength/Dexterity same-Power sign-flip, worked examples from the plan.
    // Structurally independent of the Un-X redesign (single power, no second power to cancel
    // against), confirmed unaffected by it.

    [Fact]
    public void ComputeSignFlip_Positive5_Invert5_Unaffected() =>
        Assert.Equal((0, 5), EmotionalExpression.ComputeSignFlip(5, 5));

    [Fact]
    public void ComputeSignFlip_NegativeOne_Invert5_YieldsPositiveOne() =>
        Assert.Equal((1, 1), EmotionalExpression.ComputeSignFlip(-1, 5));

    [Fact]
    public void ComputeSignFlip_NegativeThree_Invert5_YieldsPositiveThree() =>
        Assert.Equal((3, 3), EmotionalExpression.ComputeSignFlip(-3, 5));

    [Fact]
    public void ComputeSignFlip_NegativeFive_Invert5_YieldsPositiveFive() =>
        Assert.Equal((5, 5), EmotionalExpression.ComputeSignFlip(-5, 5));

    [Fact]
    public void ComputeSignFlip_NegativeSix_Invert5_YieldsPositiveFour() =>
        Assert.Equal((5, 4), EmotionalExpression.ComputeSignFlip(-6, 5));

    [Fact]
    public void ComputeSignFlip_NegativeTen_Invert5_YieldsZero() =>
        Assert.Equal((5, 0), EmotionalExpression.ComputeSignFlip(-10, 5));

    [Fact]
    public void ComputeSignFlip_NegativeTwenty_Invert5_YieldsNegativeTen() =>
        Assert.Equal((5, -10), EmotionalExpression.ComputeSignFlip(-20, 5));

    [Fact]
    public void ComputeSignFlip_Zero_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeSignFlip(0, 5));
}
