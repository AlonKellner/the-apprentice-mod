using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class TransposeTests
{
    [Fact]
    public void Transpose_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Transpose", Transpose.CardId);
}
