using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class VirtuosoTests
{
    [Fact]
    public void Virtuoso_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Virtuoso", Virtuoso.CardId);
}
