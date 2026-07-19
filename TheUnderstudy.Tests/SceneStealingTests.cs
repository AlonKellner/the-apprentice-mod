using System.Collections.Generic;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure logic for the reworked Swap mechanic. The registries, recency observer, and Swap flow itself
// need ModelDb/combat (not available in the bare test host — see AssemblyInfo.cs), so only the extracted
// math is unit-tested here; the full give/take flow is verified in-game.
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

    // SelectByRecency — among candidates given in registry order, pick the most recently modified
    // (highest recency stamp); with no stamps at all, fall back to the first (registry order).

    [Fact]
    public void SelectByRecency_Empty_ReturnsMinusOne() =>
        Assert.Equal(-1, SceneStealing.SelectByRecency(new List<long>()));

    [Fact]
    public void SelectByRecency_PicksHighestStamp() =>
        // index 1 has the newest modification
        Assert.Equal(1, SceneStealing.SelectByRecency(new List<long> { 5, 9, 7 }));

    [Fact]
    public void SelectByRecency_AllUnrecorded_FallsBackToFirst() =>
        // long.MinValue = "never recorded" for every candidate -> registry order (first)
        Assert.Equal(0, SceneStealing.SelectByRecency(new List<long> { long.MinValue, long.MinValue }));

    [Fact]
    public void SelectByRecency_RecordedBeatsUnrecorded() =>
        // A recorded candidate (index 2) wins over unrecorded ones even if it's later in registry order.
        Assert.Equal(2, SceneStealing.SelectByRecency(new List<long> { long.MinValue, long.MinValue, 1 }));

    [Fact]
    public void SelectByRecency_SingleCandidate_ReturnsIt() =>
        Assert.Equal(0, SceneStealing.SelectByRecency(new List<long> { long.MinValue }));
}
