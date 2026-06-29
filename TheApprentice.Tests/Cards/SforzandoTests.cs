using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class SforzandoTests
{
    [Fact]
    public void Sforzando_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Sforzando", Sforzando.CardId);
}
