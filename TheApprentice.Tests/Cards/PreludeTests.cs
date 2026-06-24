using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class PreludeTests
{
    [Fact]
    public void Prelude_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Prelude", Prelude.CardId);
}
