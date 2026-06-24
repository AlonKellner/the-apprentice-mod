using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class RealizeTests
{
    [Fact]
    public void Realize_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Realize", Realize.CardId);
}
