using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
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

    // HasAnyInvertibleDebuffPresent — pure core behind the "is there anything for Invert to act
    // on right now" check, used by relevance-highlighting on Coda/Reprise/SteadyNow/TakeABreath/
    // EverythingIveGot. Covers all 8 invertible pairs (the 5 self-debuffs Understudy's own cards
    // generate, plus Frail/Strength/Dexterity which Invert must still recognize).

    [Fact]
    public void HasAnyInvertibleDebuffPresent_AllZero_ReturnsFalse() =>
        Assert.False(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyWeak_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(3, 0, 0, 0, 0, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyVulnerable_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 2, 0, 0, 0, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyShaken_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 1, 0, 0, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyLimited_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 1, 0, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyJaded_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 1, 0, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_OnlyFrail_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 1, 0, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_NegativeStrength_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 0, -1, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_PositiveStrength_ReturnsFalse() =>
        Assert.False(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 0, 3, 0));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_NegativeDexterity_ReturnsTrue() =>
        Assert.True(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 0, 0, -1));

    [Fact]
    public void HasAnyInvertibleDebuffPresent_PositiveDexterity_ReturnsFalse() =>
        Assert.False(EmotionalExpression.HasAnyInvertibleDebuffPresent(0, 0, 0, 0, 0, 0, 0, 2));

    // IdentifyPair — used by My Own Lesson's reversed-mode swap on InvertTrackerPower to find the
    // opposite power for one of the 6 real X/Un-X pairs. Strength/Dexterity deliberately return null
    // here (no separate Un-X power to redirect to — handled by a distinct sign-flip branch instead).

    [Fact]
    public void IdentifyPair_WeakPower_IsDebuffSideOfWeak() =>
        Assert.Equal((InvertibleDebuff.Weak, true), EmotionalExpression.IdentifyPair(new WeakPower()));

    [Fact]
    public void IdentifyPair_UnweakPower_IsBuffSideOfWeak() =>
        Assert.Equal((InvertibleDebuff.Weak, false), EmotionalExpression.IdentifyPair(new UnweakPower()));

    [Fact]
    public void IdentifyPair_VulnerablePower_IsDebuffSideOfVulnerable() =>
        Assert.Equal((InvertibleDebuff.Vulnerable, true), EmotionalExpression.IdentifyPair(new VulnerablePower()));

    [Fact]
    public void IdentifyPair_UnvulnerablePower_IsBuffSideOfVulnerable() =>
        Assert.Equal((InvertibleDebuff.Vulnerable, false), EmotionalExpression.IdentifyPair(new UnvulnerablePower()));

    [Fact]
    public void IdentifyPair_ShakenPower_IsDebuffSideOfShaken() =>
        Assert.Equal((InvertibleDebuff.Shaken, true), EmotionalExpression.IdentifyPair(new ShakenPower()));

    [Fact]
    public void IdentifyPair_UnshakenPower_IsBuffSideOfShaken() =>
        Assert.Equal((InvertibleDebuff.Shaken, false), EmotionalExpression.IdentifyPair(new UnshakenPower()));

    [Fact]
    public void IdentifyPair_LimitedPower_IsDebuffSideOfLimited() =>
        Assert.Equal((InvertibleDebuff.Limited, true), EmotionalExpression.IdentifyPair(new LimitedPower()));

    [Fact]
    public void IdentifyPair_UnlimitedPower_IsBuffSideOfLimited() =>
        Assert.Equal((InvertibleDebuff.Limited, false), EmotionalExpression.IdentifyPair(new UnlimitedPower()));

    [Fact]
    public void IdentifyPair_JadedPower_IsDebuffSideOfJaded() =>
        Assert.Equal((InvertibleDebuff.Jaded, true), EmotionalExpression.IdentifyPair(new JadedPower()));

    [Fact]
    public void IdentifyPair_UnjadedPower_IsBuffSideOfJaded() =>
        Assert.Equal((InvertibleDebuff.Jaded, false), EmotionalExpression.IdentifyPair(new UnjadedPower()));

    [Fact]
    public void IdentifyPair_FrailPower_IsDebuffSideOfFrail() =>
        Assert.Equal((InvertibleDebuff.Frail, true), EmotionalExpression.IdentifyPair(new FrailPower()));

    [Fact]
    public void IdentifyPair_UnfrailPower_IsBuffSideOfFrail() =>
        Assert.Equal((InvertibleDebuff.Frail, false), EmotionalExpression.IdentifyPair(new UnfrailPower()));

    [Fact]
    public void IdentifyPair_StrengthPower_ReturnsNull() =>
        Assert.Null(EmotionalExpression.IdentifyPair(new StrengthPower()));

    [Fact]
    public void IdentifyPair_DexterityPower_ReturnsNull() =>
        Assert.Null(EmotionalExpression.IdentifyPair(new DexterityPower()));

    // Categorize / CategorizeSigned — Second Lesson's Reward/Punish 3-way pair categorization.

    [Fact]
    public void Categorize_BothZero_IsNone() =>
        Assert.Equal(EmotionalExpression.PairCategory.None, EmotionalExpression.Categorize(0, 0));

    [Fact]
    public void Categorize_DebuffPositive_IsDebuffPresent() =>
        Assert.Equal(EmotionalExpression.PairCategory.DebuffPresent, EmotionalExpression.Categorize(3, 0));

    [Fact]
    public void Categorize_BuffPositive_IsBuffPresent() =>
        Assert.Equal(EmotionalExpression.PairCategory.BuffPresent, EmotionalExpression.Categorize(0, 2));

    [Fact]
    public void Categorize_BothPositive_DebuffWins() =>
        Assert.Equal(EmotionalExpression.PairCategory.DebuffPresent, EmotionalExpression.Categorize(1, 1));

    [Fact]
    public void CategorizeSigned_Zero_IsNone() =>
        Assert.Equal(EmotionalExpression.PairCategory.None, EmotionalExpression.CategorizeSigned(0));

    [Fact]
    public void CategorizeSigned_Negative_IsDebuffPresent() =>
        Assert.Equal(EmotionalExpression.PairCategory.DebuffPresent, EmotionalExpression.CategorizeSigned(-2));

    [Fact]
    public void CategorizeSigned_Positive_IsBuffPresent() =>
        Assert.Equal(EmotionalExpression.PairCategory.BuffPresent, EmotionalExpression.CategorizeSigned(3));

    // RewardPriority / PunishPriority — exact order matters.

    [Fact]
    public void RewardPriority_IsNoneThenBuffThenDebuff() => Assert.Equal(
        new[] { EmotionalExpression.PairCategory.None, EmotionalExpression.PairCategory.BuffPresent, EmotionalExpression.PairCategory.DebuffPresent },
        EmotionalExpression.RewardPriority);

    [Fact]
    public void PunishPriority_IsNoneThenDebuffThenBuff() => Assert.Equal(
        new[] { EmotionalExpression.PairCategory.None, EmotionalExpression.PairCategory.DebuffPresent, EmotionalExpression.PairCategory.BuffPresent },
        EmotionalExpression.PunishPriority);

    // PickByPriority — the core of Reward/Punish's pair-selection identity.

    [Fact]
    public void PickByPriority_SomeNonePresent_PicksFromNoneTier()
    {
        var categories = new Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory>
        {
            [InvertibleDebuff.Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [InvertibleDebuff.Vulnerable] = EmotionalExpression.PairCategory.None,
            [InvertibleDebuff.Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[0]);
        Assert.Equal(InvertibleDebuff.Vulnerable, result);
    }

    [Fact]
    public void PickByPriority_Reward_NoneAbsent_PrefersBuffOverDebuff()
    {
        var categories = new Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory>
        {
            [InvertibleDebuff.Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [InvertibleDebuff.Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[0]);
        Assert.Equal(InvertibleDebuff.Shaken, result);
    }

    [Fact]
    public void PickByPriority_Punish_NoneAbsent_PrefersDebuffOverBuff()
    {
        var categories = new Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory>
        {
            [InvertibleDebuff.Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [InvertibleDebuff.Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.PunishPriority, list => list[0]);
        Assert.Equal(InvertibleDebuff.Weak, result);
    }

    [Fact]
    public void PickByPriority_OnlyLeastPreferredTierNonEmpty_FallsThroughToIt()
    {
        var categories = new Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory>
        {
            [InvertibleDebuff.Weak] = EmotionalExpression.PairCategory.DebuffPresent,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[0]);
        Assert.Equal(InvertibleDebuff.Weak, result);
    }

    [Fact]
    public void PickByPriority_UsesInjectedPickerAmongTiedCandidates()
    {
        var categories = new Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory>
        {
            [InvertibleDebuff.Weak] = EmotionalExpression.PairCategory.None,
            [InvertibleDebuff.Vulnerable] = EmotionalExpression.PairCategory.None,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[^1]);
        Assert.Equal(InvertibleDebuff.Vulnerable, result);
    }

    // ExcludeForPunishIfFirstLessonActive — Weak/Vulnerable are silent no-ops as punishments while
    // The First Lesson is active, so Punish's pool must drop them entirely (not just deprioritize).

    private static Dictionary<InvertibleDebuff, EmotionalExpression.PairCategory> AllPairsNone() =>
        Enum.GetValues<InvertibleDebuff>().ToDictionary(d => d, _ => EmotionalExpression.PairCategory.None);

    [Fact]
    public void ExcludeForPunishIfFirstLessonActive_NotActive_LeavesAllPairs()
    {
        var categories = AllPairsNone();
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(categories, firstLessonActive: false);
        Assert.Equal(8, filtered.Count);
    }

    [Fact]
    public void ExcludeForPunishIfFirstLessonActive_Active_RemovesWeakAndVulnerableOnly()
    {
        var categories = AllPairsNone();
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(categories, firstLessonActive: true);
        Assert.Equal(6, filtered.Count);
        Assert.DoesNotContain(InvertibleDebuff.Weak, filtered.Keys);
        Assert.DoesNotContain(InvertibleDebuff.Vulnerable, filtered.Keys);
        Assert.Contains(InvertibleDebuff.Shaken, filtered.Keys);
        Assert.Contains(InvertibleDebuff.Strength, filtered.Keys);
    }
}
