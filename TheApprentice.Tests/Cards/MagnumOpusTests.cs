using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class MagnumOpusTests
{
    [Fact]
    public void MagnumOpus_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:MagnumOpus", MagnumOpus.CardId);
}
