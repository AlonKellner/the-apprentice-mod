using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class GrooveTests
{
    [Fact]
    public void Groove_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Groove", Groove.CardId);
}
