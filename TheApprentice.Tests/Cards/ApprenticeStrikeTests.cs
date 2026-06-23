using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ApprenticeStrikeTests
{
    [Fact]
    public void ApprenticeStrike_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:ApprenticeStrike", ApprenticeStrike.CardId);
    }
}
