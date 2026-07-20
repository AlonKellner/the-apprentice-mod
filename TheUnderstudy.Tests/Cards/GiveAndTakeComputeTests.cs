using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Pure "simultaneous" math for Give & Take: from one debuff of magnitude `amount`, Swap gives up to
// `swapCap` to each enemy and Invert flips up to `invertMax` onto you, both read from the same snapshot,
// and the self-removal is capped at `amount` (never below zero). The live apply is verified in-game.
public class GiveAndTakeComputeTests
{
    [Theory]
    // amount, swapCap, invertMax => swapRemove, invertRemove, selfRemove
    [InlineData(1, 10, 1, 1, 1, 1)]   // the reported case: 1 Weak -> you Unweak, enemies Weak, Weak stripped
    [InlineData(5, 10, 1, 5, 1, 5)]   // 5 Weak: enemies +5, you +1 Unweak, all 5 stripped (swap alone clears it)
    [InlineData(10, 10, 2, 10, 2, 10)] // upgraded invert flips 2
    [InlineData(3, 0, 1, 0, 1, 1)]    // not swappable (swapCap 0): invert-only -> flip 1, strip 1
    [InlineData(3, 10, 0, 3, 0, 3)]   // not invertible (invertMax 0): swap-only -> give 3, strip 3
    [InlineData(0, 10, 2, 0, 0, 0)]   // no debuff: nothing happens
    [InlineData(2, 1, 1, 1, 1, 2)]    // small cap: swap 1 + invert 1 = strip 2 (the whole stack)
    public void ComputeGiveAndTake_MatchesSnapshotSemantics(
        int amount, int swapCap, int invertMax, int expSwap, int expInvert, int expSelfRemove)
    {
        var (swapRemove, invertRemove, selfRemove) =
            InvertiblePairs.ComputeGiveAndTake(amount, swapCap, invertMax);
        Assert.Equal(expSwap, swapRemove);
        Assert.Equal(expInvert, invertRemove);
        Assert.Equal(expSelfRemove, selfRemove);
    }
}
