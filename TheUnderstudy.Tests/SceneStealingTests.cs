using System.Collections.Generic;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure logic for the reworked Swap mechanic. The registries and Swap flow itself need ModelDb/combat
// (not available in the bare test host — see AssemblyInfo.cs), so only the extracted math is unit-tested
// here; the full give/take flow is verified in-game.
public class SceneStealingTests
{
    // ComputeTransfer — how much of a single holding moves per Swap application: the WHOLE stack, never
    // negative. Swap no longer caps the amount (was SwapCap = 10); a stack of any size moves in full. Also
    // used for a sign-flip buff's negative magnitude.

    [Fact]
    public void ComputeTransfer_LargeStack_MovesTheWholeThing() =>
        Assert.Equal(25, SceneStealing.ComputeTransfer(have: 25));

    [Fact]
    public void ComputeTransfer_SmallStack_MovesAllYouHave() =>
        Assert.Equal(3, SceneStealing.ComputeTransfer(have: 3));

    [Fact]
    public void ComputeTransfer_HaveNone_MovesNothing() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: 0));

    [Fact]
    public void ComputeTransfer_NegativeHolding_NeverNegative() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: -4));

    // SelectRightmostN — the N distinct powers a "Swap N times" moves: the n candidates sitting rightmost
    // (largest positions) in the creature's Powers list, rightmost first. Deterministic on every client.

    [Fact]
    public void SelectRightmostN_PicksTheNHighestPositions_RightmostFirst() =>
        // positions 5,1,9,3 -> the two rightmost are index 2 (pos 9) then index 0 (pos 5)
        Assert.Equal(new[] { 2, 0 }, SceneStealing.SelectRightmostN(new List<int> { 5, 1, 9, 3 }, 2));

    [Fact]
    public void SelectRightmostN_NGreaterThanCount_ReturnsAllRightmostFirst() =>
        Assert.Equal(new[] { 2, 0, 1 }, SceneStealing.SelectRightmostN(new List<int> { 4, 1, 7 }, 10));

    [Fact]
    public void SelectRightmostN_One_MatchesSelectRightmost() =>
        Assert.Equal(new[] { 2 }, SceneStealing.SelectRightmostN(new List<int> { 5, 1, 9 }, 1));

    [Fact]
    public void SelectRightmostN_ZeroOrEmpty_ReturnsNothing()
    {
        Assert.Empty(SceneStealing.SelectRightmostN(new List<int> { 5, 1, 9 }, 0));
        Assert.Empty(SceneStealing.SelectRightmostN(new List<int>(), 3));
    }

    [Fact]
    public void SelectRightmostN_Ties_PreferLaterCandidate() =>
        // equal positions -> later index first (matches SelectRightmost's >= tie-break)
        Assert.Equal(new[] { 2, 1 }, SceneStealing.SelectRightmostN(new List<int> { 4, 4, 4 }, 2));

    // SelfGiveDelta / EnemyGiveDelta — the direction a give moves the number, and the only place that
    // knows it. Both regular Swap and Best of Both build their give moves from these, which is what stops
    // the two from disagreeing about sign-flip powers the way they used to.

    [Fact]
    public void GiveDeltas_NormalDebuff_LeavesYouAndLandsOnEnemy()
    {
        // Weak: 3 removed from you, 3 added to each enemy.
        Assert.Equal(-3, SceneStealing.SelfGiveDelta(signFlip: false, 3));
        Assert.Equal(3, SceneStealing.EnemyGiveDelta(signFlip: false, 3));
    }

    [Fact]
    public void GiveDeltas_SignFlipDebuff_NudgesYouTowardZeroAndPilesNegativeOnEnemy()
    {
        // Negative Vigor: the debuff is the negative portion, so giving it moves YOU up toward zero and
        // pushes the enemy further down. Opposite signs to a normal debuff — the case Best of Both missed.
        Assert.Equal(3, SceneStealing.SelfGiveDelta(signFlip: true, 3));
        Assert.Equal(-3, SceneStealing.EnemyGiveDelta(signFlip: true, 3));
    }

    [Fact]
    public void GiveDeltas_NothingToGive_MoveNothing()
    {
        Assert.Equal(0, SceneStealing.SelfGiveDelta(signFlip: true, 0));
        Assert.Equal(0, SceneStealing.EnemyGiveDelta(signFlip: false, 0));
    }

    // SelectRightmost — among candidates, pick the one sitting RIGHTMOST (highest index) in the creature's
    // Powers list. positions[i] is candidate i's index within creature.Powers (-1 if absent). The Powers
    // list order is checksummed and synced, so this is deterministic on every multiplayer client.

    [Fact]
    public void SelectRightmost_Empty_ReturnsMinusOne() =>
        Assert.Equal(-1, SceneStealing.SelectRightmost(new List<int>()));

    [Fact]
    public void SelectRightmost_PicksHighestPosition() =>
        // candidate at index 2 sits last (position 9) in the Powers list
        Assert.Equal(2, SceneStealing.SelectRightmost(new List<int> { 5, 1, 9 }));

    [Fact]
    public void SelectRightmost_SingleCandidate_ReturnsIt() =>
        Assert.Equal(0, SceneStealing.SelectRightmost(new List<int> { 3 }));

    [Fact]
    public void SelectRightmost_Ties_PrefersLaterCandidate() =>
        // equal positions -> the later candidate wins (>= comparison)
        Assert.Equal(1, SceneStealing.SelectRightmost(new List<int> { 4, 4 }));

    [Fact]
    public void SelectRightmost_AbsentCandidatesIgnored() =>
        // -1 = "not held"; the only real position (index 1) wins over absent ones
        Assert.Equal(1, SceneStealing.SelectRightmost(new List<int> { -1, 0, -1 }));
}
