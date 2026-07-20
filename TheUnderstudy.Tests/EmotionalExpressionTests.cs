using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure Second-Lesson/Invert selection math. Pair membership/kind is covered by InvertiblePairsTests; the
// live per-creature Apply/Invert/Categorize is verified in-game (needs ModelDb/combat).
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
    public void CountUniqueDebuffTypes_BothPresent_Returns2() =>
        Assert.Equal(2, EmotionalExpression.CountUniqueDebuffTypes(2, 4));

    // ComputeWeakCancellation — the shared cancellation primitive InvertTrackerPower leans on for every
    // same-shape pair, both directions.

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
    public void ComputeWeakCancellation_ZeroApplied_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeWeakCancellation(0, 4));

    // ComputeSignFlip — Strength/Dexterity/Vigor same-Power sign-flip, worked examples from the plan.

    [Fact]
    public void ComputeSignFlip_Positive5_Invert5_Unaffected() =>
        Assert.Equal((0, 5), EmotionalExpression.ComputeSignFlip(5, 5));

    [Fact]
    public void ComputeSignFlip_NegativeThree_Invert5_YieldsPositiveThree() =>
        Assert.Equal((3, 3), EmotionalExpression.ComputeSignFlip(-3, 5));

    [Fact]
    public void ComputeSignFlip_NegativeSix_Invert5_YieldsPositiveFour() =>
        Assert.Equal((5, 4), EmotionalExpression.ComputeSignFlip(-6, 5));

    [Fact]
    public void ComputeSignFlip_NegativeTwenty_Invert5_YieldsNegativeTen() =>
        Assert.Equal((5, -10), EmotionalExpression.ComputeSignFlip(-20, 5));

    [Fact]
    public void ComputeSignFlip_Zero_NoOp() =>
        Assert.Equal((0, 0), EmotionalExpression.ComputeSignFlip(0, 5));

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

    // PickByPriority — the core of Reward/Punish's pair-selection identity, now keyed on InvertiblePair.

    private static readonly InvertiblePair Weak = InvertiblePairs.For(new WeakPower())!;
    private static readonly InvertiblePair Vulnerable = InvertiblePairs.For(new VulnerablePower())!;
    private static readonly InvertiblePair Shaken = InvertiblePairs.For(new ShakenPower())!;

    [Fact]
    public void PickByPriority_SomeNonePresent_PicksFromNoneTier()
    {
        var categories = new Dictionary<InvertiblePair, EmotionalExpression.PairCategory>
        {
            [Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [Vulnerable] = EmotionalExpression.PairCategory.None,
            [Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        var result = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[0]);
        Assert.Same(Vulnerable, result);
    }

    [Fact]
    public void PickByPriority_Reward_NoneAbsent_PrefersBuffOverDebuff()
    {
        var categories = new Dictionary<InvertiblePair, EmotionalExpression.PairCategory>
        {
            [Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        Assert.Same(Shaken, EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[0]));
    }

    [Fact]
    public void PickByPriority_Punish_NoneAbsent_PrefersDebuffOverBuff()
    {
        var categories = new Dictionary<InvertiblePair, EmotionalExpression.PairCategory>
        {
            [Weak] = EmotionalExpression.PairCategory.DebuffPresent,
            [Shaken] = EmotionalExpression.PairCategory.BuffPresent,
        };
        Assert.Same(Weak, EmotionalExpression.PickByPriority(categories, EmotionalExpression.PunishPriority, list => list[0]));
    }

    [Fact]
    public void PickByPriority_UsesInjectedPickerAmongTiedCandidates()
    {
        var categories = new Dictionary<InvertiblePair, EmotionalExpression.PairCategory>
        {
            [Weak] = EmotionalExpression.PairCategory.None,
            [Vulnerable] = EmotionalExpression.PairCategory.None,
        };
        Assert.Same(Vulnerable, EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, list => list[^1]));
    }

    // ExcludeForPunishIfFirstLessonActive — Weak/Vulnerable are silent no-ops as punishments while The First
    // Lesson is active, so Punish's pool drops exactly those two pairs (not just deprioritize).

    private static Dictionary<InvertiblePair, EmotionalExpression.PairCategory> AllPairsNone() =>
        InvertiblePairs.All.ToDictionary(p => p, _ => EmotionalExpression.PairCategory.None);

    [Fact]
    public void ExcludeForPunishIfFirstLessonActive_NotActive_LeavesAllPairs()
    {
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(AllPairsNone(), firstLessonActive: false);
        Assert.Equal(InvertiblePairs.All.Count, filtered.Count);
    }

    [Fact]
    public void ExcludeForPunishIfFirstLessonActive_Active_RemovesWeakAndVulnerableOnly()
    {
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(AllPairsNone(), firstLessonActive: true);
        Assert.Equal(InvertiblePairs.All.Count - 2, filtered.Count);
        Assert.DoesNotContain(Weak, filtered.Keys);
        Assert.DoesNotContain(Vulnerable, filtered.Keys);
        Assert.Contains(Shaken, filtered.Keys);
        Assert.Contains(InvertiblePairs.For(new StrengthPower())!, filtered.Keys);
    }
}
