using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ImproviseTests
{
    [Fact]
    public void Improvise_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Improvise", Improvise.CardId);
}
