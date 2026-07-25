using System.Collections.Generic;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure logic for the reworked Swap mechanic. The registries and Swap flow itself need ModelDb/combat
// (not available in the bare test host — see AssemblyInfo.cs), so only the extracted math is unit-tested
// here; the full give/take flow is verified in-game.
public class SceneStealingTests
{
    // ComputeTransfer — how much of a single holding moves per Swap application: capped at SwapCap (10)
    // and at what you actually have, never negative. Also used for a sign-flip buff's negative magnitude.

    [Fact]
    public void ComputeTransfer_HaveMoreThanCap_MovesCap() =>
        Assert.Equal(SceneStealing.SwapCap, SceneStealing.ComputeTransfer(have: 25));

    [Fact]
    public void ComputeTransfer_HaveLessThanCap_MovesAllYouHave() =>
        Assert.Equal(3, SceneStealing.ComputeTransfer(have: 3));

    [Fact]
    public void ComputeTransfer_HaveExactlyCap_MovesCap() =>
        Assert.Equal(10, SceneStealing.ComputeTransfer(have: 10));

    [Fact]
    public void ComputeTransfer_HaveNone_MovesNothing() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: 0));

    [Fact]
    public void ComputeTransfer_NegativeHolding_NeverNegative() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: -4));

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
