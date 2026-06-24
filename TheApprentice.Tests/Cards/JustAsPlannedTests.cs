using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class JustAsPlannedTests
{
    [Fact]
    public void JustAsPlanned_CardId_MatchesExpectedConstant()
    {
        Assert.Equal("TheApprentice:JustAsPlanned", JustAsPlanned.CardId);
    }
}
