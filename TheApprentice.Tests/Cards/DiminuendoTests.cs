using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class DiminuendoTests
{
    [Fact]
    public void Diminuendo_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Diminuendo", Diminuendo.CardId);
}
