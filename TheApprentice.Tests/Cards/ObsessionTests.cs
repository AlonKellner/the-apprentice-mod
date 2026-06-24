using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ObsessionTests
{
    [Fact]
    public void Obsession_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Obsession", Obsession.CardId);
}
