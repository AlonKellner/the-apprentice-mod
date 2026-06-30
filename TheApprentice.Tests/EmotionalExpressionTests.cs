using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests;

public class EmotionalExpressionTests
{
    // ComputeNetWeak — apply Unweak to a creature with existing Weak

    [Fact]
    public void ComputeNetWeak_NoExistingWeak_ApplyUnweak3_YieldsUnweak3() =>
        Assert.Equal((0, 3), EmotionalExpression.ComputeNetWeak(0, 0, 0, 3));

    [Fact]
    public void ComputeNetWeak_Weak2_ApplyUnweak1_LeavesWeak1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetWeak(2, 0, 0, 1));

    [Fact]
    public void ComputeNetWeak_Weak2_ApplyUnweak2_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetWeak(2, 0, 0, 2));

    [Fact]
    public void ComputeNetWeak_Weak2_ApplyUnweak3_LeavesUnweak1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetWeak(2, 0, 0, 3));

    // ComputeNetWeak — apply Weak to a creature with existing Unweak

    [Fact]
    public void ComputeNetWeak_Unweak2_ApplyWeak1_LeavesUnweak1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetWeak(0, 2, 1, 0));

    [Fact]
    public void ComputeNetWeak_Unweak2_ApplyWeak2_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetWeak(0, 2, 2, 0));

    [Fact]
    public void ComputeNetWeak_Unweak2_ApplyWeak3_LeavesWeak1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetWeak(0, 2, 3, 0));

    // ComputeNetWeak — both present simultaneously

    [Fact]
    public void ComputeNetWeak_Weak3_Unweak1_ApplyUnweak2_LeavesWeak0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetWeak(3, 1, 0, 2));

    // ComputeWeakConversion — "convert up to max Weak to Unweak". Capped conversions that leave a
    // remainder must cancel that remainder against the newly-created Unweak rather than letting
    // both coexist (Weak/Unweak are documented as mutually-cancelling, see UnweakPower's tooltip).

    [Fact]
    public void ComputeWeakConversion_Weak1_Max1_ConvertsFully() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeWeakConversion(1, 0, 1));

    [Fact]
    public void ComputeWeakConversion_Weak2_Max1_RemainderCancelsNewUnweak() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeWeakConversion(2, 0, 1));

    [Fact]
    public void ComputeWeakConversion_Weak3_Max1_RemainderPartiallyCancels() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeWeakConversion(3, 0, 1));

    [Fact]
    public void ComputeWeakConversion_Weak3_Max2_LeftoverWeak1CancelsOneNewUnweak() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeWeakConversion(3, 0, 2));

    [Fact]
    public void ComputeWeakConversion_Weak2_Max5_ConvertsAll_NoLeftover() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeWeakConversion(2, 0, 5));

    [Fact]
    public void ComputeWeakConversion_NoWeak_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeWeakConversion(0, 0, 1));

    [Fact]
    public void ComputeWeakConversion_MaxZero_NoOp() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeWeakConversion(3, 0, 0));

    // ComputeVulnerableConversion — mirror cases

    [Fact]
    public void ComputeVulnerableConversion_Vulnerable1_Max1_ConvertsFully() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeVulnerableConversion(1, 0, 1));

    [Fact]
    public void ComputeVulnerableConversion_Vulnerable2_Max1_RemainderCancelsNewUnvulnerable() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeVulnerableConversion(2, 0, 1));

    [Fact]
    public void ComputeVulnerableConversion_Vulnerable3_Max1_RemainderPartiallyCancels() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeVulnerableConversion(3, 0, 1));

    [Fact]
    public void ComputeVulnerableConversion_Vulnerable3_Max2_LeftoverVulnerable1CancelsOneNewUnvulnerable() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeVulnerableConversion(3, 0, 2));

    [Fact]
    public void ComputeVulnerableConversion_Vulnerable2_Max5_ConvertsAll_NoLeftover() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeVulnerableConversion(2, 0, 5));

    [Fact]
    public void ComputeVulnerableConversion_NoVulnerable_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeVulnerableConversion(0, 0, 1));

    [Fact]
    public void ComputeVulnerableConversion_MaxZero_NoOp() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeVulnerableConversion(3, 0, 0));

    // ComputeNetVulnerable — mirror cases

    [Fact]
    public void ComputeNetVulnerable_NoExisting_ApplyUnvulnerable2_YieldsUnvulnerable2() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeNetVulnerable(0, 0, 0, 2));

    [Fact]
    public void ComputeNetVulnerable_Vulnerable3_ApplyUnvulnerable2_LeavesVulnerable1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetVulnerable(3, 0, 0, 2));

    [Fact]
    public void ComputeNetVulnerable_Vulnerable3_ApplyUnvulnerable3_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetVulnerable(3, 0, 0, 3));

    [Fact]
    public void ComputeNetVulnerable_Vulnerable3_ApplyUnvulnerable4_LeavesUnvulnerable1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetVulnerable(3, 0, 0, 4));

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

    // ComputeWeakCancellation — incoming Weak absorbed by existing Unweak

    [Fact]
    public void ComputeWeakCancellation_NoUnweak_PassesThrough() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeWeakCancellation(3, 0));

    [Fact]
    public void ComputeWeakCancellation_ExactMatch_BlocksAll() =>
        Assert.Equal((0, 3), EmotionalExpression.ComputeWeakCancellation(3, 3));

    [Fact]
    public void ComputeWeakCancellation_MoreWeakThanUnweak_ReducesPartially() =>
        Assert.Equal((1, 2), EmotionalExpression.ComputeWeakCancellation(3, 2));

    [Fact]
    public void ComputeWeakCancellation_MoreUnweakThanWeak_BlocksAll() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeWeakCancellation(2, 5));
}
