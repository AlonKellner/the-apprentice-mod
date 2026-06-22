using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ScrapPlansTests
{
    [Fact]
    public void ScrapPlans_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:ScrapPlans", ScrapPlans.CardId);
    }
}
