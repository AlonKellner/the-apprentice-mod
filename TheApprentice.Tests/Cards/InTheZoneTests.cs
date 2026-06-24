using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class InTheZoneTests
{
    [Fact]
    public void InTheZone_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:InTheZone", InTheZone.CardId);
}
