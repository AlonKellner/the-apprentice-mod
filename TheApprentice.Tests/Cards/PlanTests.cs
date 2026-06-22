using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class PlanTests
{
    [Fact]
    public void Plan_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:Plan", Plan.CardId);
    }
}
