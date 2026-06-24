using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class EpiphanyTests
{
    [Fact]
    public void Epiphany_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Epiphany", Epiphany.CardId);
}
