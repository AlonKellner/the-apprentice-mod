using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class FlatTests
{
    [Fact]
    public void Flat_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Flat", Flat.CardId);
}
