using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ExecutePlansTests
{
    [Fact]
    public void ExecutePlans_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:ExecutePlans", ExecutePlans.CardId);
    }
}
