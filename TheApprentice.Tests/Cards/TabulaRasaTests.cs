using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class TabulaRasaTests
{
    [Fact]
    public void TabulaRasa_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:TabulaRasa", TabulaRasa.CardId);
}
