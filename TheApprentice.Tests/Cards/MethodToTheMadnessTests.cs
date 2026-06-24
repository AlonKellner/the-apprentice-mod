using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class MethodToTheMadnessTests
{
    [Fact]
    public void MethodToTheMadness_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:MethodToTheMadness", MethodToTheMadness.CardId);
}
