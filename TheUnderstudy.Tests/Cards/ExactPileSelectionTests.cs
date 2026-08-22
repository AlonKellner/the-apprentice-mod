using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// The pile-selection helper shows the selection screen ONLY when there are more eligible cards than
// required; otherwise it auto-applies to all candidates (even if fewer than required). Pure boundary check
// for that decision — the surrounding CardSelectCmd/UI path is verified in-game.
public class ExactPileSelectionTests
{
    [Theory]
    [InlineData(3, 2, true)]   // more candidates than required -> prompt to pick exactly 2
    [InlineData(2, 2, false)]  // exactly required -> auto-apply both, no prompt
    [InlineData(1, 2, false)]  // fewer than required -> auto-apply the one
    [InlineData(0, 2, false)]  // none -> nothing to apply
    [InlineData(2, 1, true)]   // single-select with 2 candidates -> prompt
    public void ShouldPrompt_OnlyWhenCandidatesExceedRequired(int candidateCount, int required, bool expected)
    {
        Assert.Equal(expected, ExactPileSelection.ShouldPrompt(candidateCount, required));
    }
}
