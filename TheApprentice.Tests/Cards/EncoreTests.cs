using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class EncoreTests
{
    [Fact]
    public void Encore_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Encore", Encore.CardId);
}
