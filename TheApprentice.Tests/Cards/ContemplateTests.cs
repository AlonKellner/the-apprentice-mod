using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ContemplateTests
{
    [Fact]
    public void Contemplate_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Contemplate", Contemplate.CardId);
}
