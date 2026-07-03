using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

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

    // ComputeLimitedConversion — mirror cases

    [Fact]
    public void ComputeLimitedConversion_Limited1_Max1_ConvertsFully() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeLimitedConversion(1, 0, 1));

    [Fact]
    public void ComputeLimitedConversion_Limited2_Max1_RemainderCancelsNewUnlimited() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeLimitedConversion(2, 0, 1));

    [Fact]
    public void ComputeLimitedConversion_Limited3_Max1_RemainderPartiallyCancels() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeLimitedConversion(3, 0, 1));

    [Fact]
    public void ComputeLimitedConversion_Limited3_Max2_LeftoverLimited1CancelsOneNewUnlimited() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeLimitedConversion(3, 0, 2));

    [Fact]
    public void ComputeLimitedConversion_Limited2_Max5_ConvertsAll_NoLeftover() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeLimitedConversion(2, 0, 5));

    [Fact]
    public void ComputeLimitedConversion_NoLimited_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeLimitedConversion(0, 0, 1));

    [Fact]
    public void ComputeLimitedConversion_MaxZero_NoOp() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeLimitedConversion(3, 0, 0));

    // ComputeNetLimited — mirror cases

    [Fact]
    public void ComputeNetLimited_NoExisting_ApplyUnlimited2_YieldsUnlimited2() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeNetLimited(0, 0, 0, 2));

    [Fact]
    public void ComputeNetLimited_Limited3_ApplyUnlimited2_LeavesLimited1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetLimited(3, 0, 0, 2));

    [Fact]
    public void ComputeNetLimited_Limited3_ApplyUnlimited3_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetLimited(3, 0, 0, 3));

    [Fact]
    public void ComputeNetLimited_Limited3_ApplyUnlimited4_LeavesUnlimited1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetLimited(3, 0, 0, 4));

    // ComputeJadedConversion — mirror cases (Jaded/Unjaded follows the same shape as Limited/Unlimited)

    [Fact]
    public void ComputeJadedConversion_Jaded1_Max1_ConvertsFully() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeJadedConversion(1, 0, 1));

    [Fact]
    public void ComputeJadedConversion_Jaded2_Max1_RemainderCancelsNewUnjaded() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeJadedConversion(2, 0, 1));

    [Fact]
    public void ComputeJadedConversion_Jaded3_Max1_RemainderPartiallyCancels() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeJadedConversion(3, 0, 1));

    [Fact]
    public void ComputeJadedConversion_Jaded3_Max2_LeftoverJaded1CancelsOneNewUnjaded() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeJadedConversion(3, 0, 2));

    [Fact]
    public void ComputeJadedConversion_Jaded2_Max5_ConvertsAll_NoLeftover() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeJadedConversion(2, 0, 5));

    [Fact]
    public void ComputeJadedConversion_NoJaded_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeJadedConversion(0, 0, 1));

    [Fact]
    public void ComputeJadedConversion_MaxZero_NoOp() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeJadedConversion(3, 0, 0));

    // ComputeNetJaded — mirror cases

    [Fact]
    public void ComputeNetJaded_NoExisting_ApplyUnjaded2_YieldsUnjaded2() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeNetJaded(0, 0, 0, 2));

    [Fact]
    public void ComputeNetJaded_Jaded3_ApplyUnjaded2_LeavesJaded1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetJaded(3, 0, 0, 2));

    [Fact]
    public void ComputeNetJaded_Jaded3_ApplyUnjaded3_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetJaded(3, 0, 0, 3));

    [Fact]
    public void ComputeNetJaded_Jaded3_ApplyUnjaded4_LeavesUnjaded1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetJaded(3, 0, 0, 4));

    // ComputeFrailConversion — mirror cases (Frail/Unfrail follows the same shape as the other 4 pairs)

    [Fact]
    public void ComputeFrailConversion_Frail1_Max1_ConvertsFully() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeFrailConversion(1, 0, 1));

    [Fact]
    public void ComputeFrailConversion_Frail2_Max1_RemainderCancelsNewUnfrail() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeFrailConversion(2, 0, 1));

    [Fact]
    public void ComputeFrailConversion_Frail3_Max1_RemainderPartiallyCancels() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeFrailConversion(3, 0, 1));

    [Fact]
    public void ComputeFrailConversion_Frail3_Max2_LeftoverFrail1CancelsOneNewUnfrail() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeFrailConversion(3, 0, 2));

    [Fact]
    public void ComputeFrailConversion_Frail2_Max5_ConvertsAll_NoLeftover() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeFrailConversion(2, 0, 5));

    [Fact]
    public void ComputeFrailConversion_NoFrail_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeFrailConversion(0, 0, 1));

    [Fact]
    public void ComputeFrailConversion_MaxZero_NoOp() =>
        Assert.Equal((3, 0), EmotionalExpression.ComputeFrailConversion(3, 0, 0));

    // ComputeNetFrail — mirror cases

    [Fact]
    public void ComputeNetFrail_NoExisting_ApplyUnfrail2_YieldsUnfrail2() =>
        Assert.Equal((0, 2), EmotionalExpression.ComputeNetFrail(0, 0, 0, 2));

    [Fact]
    public void ComputeNetFrail_Frail3_ApplyUnfrail2_LeavesFrail1() =>
        Assert.Equal((1, 0), EmotionalExpression.ComputeNetFrail(3, 0, 0, 2));

    [Fact]
    public void ComputeNetFrail_Frail3_ApplyUnfrail3_LeavesBoth0() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeNetFrail(3, 0, 0, 3));

    [Fact]
    public void ComputeNetFrail_Frail3_ApplyUnfrail4_LeavesUnfrail1() =>
        Assert.Equal((0, 1), EmotionalExpression.ComputeNetFrail(3, 0, 0, 4));

    // ComputeSignFlip — Strength/Dexterity same-Power sign-flip, worked examples from the plan

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
