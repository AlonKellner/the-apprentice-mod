using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ClearMindTests
{
    [Fact]
    public void ClearMind_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:ClearMind", ClearMind.CardId);
}
