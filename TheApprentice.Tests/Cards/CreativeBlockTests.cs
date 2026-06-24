using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class CreativeBlockTests
{
    [Fact]
    public void CreativeBlock_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:CreativeBlock", CreativeBlock.CardId);
}
