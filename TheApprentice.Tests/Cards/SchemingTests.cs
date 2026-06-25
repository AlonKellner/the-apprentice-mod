using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class SchemingTests
{
    [Fact]
    public void Scheming_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Scheming", Scheming.CardId);

    [Fact]
    public void Scheming_BaseCostIsTwo() =>
        Assert.Equal(2, new Scheming().EnergyCost.Canonical);
}
