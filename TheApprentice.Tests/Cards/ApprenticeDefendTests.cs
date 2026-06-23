using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ApprenticeDefendTests
{
    [Fact]
    public void ApprenticeDefend_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:ApprenticeDefend", ApprenticeDefend.CardId);
    }
}
