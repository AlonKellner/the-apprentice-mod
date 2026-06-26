using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class VirtuosoTests
{
    [Fact]
    public void Virtuoso_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Virtuoso", Virtuoso.CardId);

    [Fact]
    public void VirtuosoPower_Localization_NonUpgraded_MentionsEndOfTurn()
    {
        var p = new VirtuosoPower();
        Assert.Contains(p.Localization, entry => entry.Item2.Contains("end of your turn"));
    }

    [Fact]
    public void VirtuosoPower_Localization_MentionsUnplayable()
    {
        var p = new VirtuosoPower();
        Assert.Contains(p.Localization, entry => entry.Item2.Contains("Unplayable"));
    }
}
