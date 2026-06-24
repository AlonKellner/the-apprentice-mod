using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class OverthinkingTests
{
    [Fact]
    public void Overthinking_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Overthinking", Overthinking.CardId);
}
