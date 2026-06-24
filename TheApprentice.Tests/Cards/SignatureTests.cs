using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class SignatureTests
{
    [Fact]
    public void Signature_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Signature", Signature.CardId);
}
