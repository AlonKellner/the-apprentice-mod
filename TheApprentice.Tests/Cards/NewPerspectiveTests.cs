using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class NewPerspectiveTests
{
    [Fact]
    public void NewPerspective_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:NewPerspective", NewPerspective.CardId);
    }
}
