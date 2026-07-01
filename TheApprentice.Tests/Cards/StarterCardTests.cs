using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

// Starter cards must use CardRarity.Basic so the game's reward system excludes them.
// showInCardLibrary=false alone only hides them from the compendium; it does not
// prevent them from appearing in combat card reward offers.
public class StarterCardTests
{
    [Fact]
    public void Plan_IsBasicRarity()
    {
        var c = new Plan();
        Assert.Equal(CardRarity.Basic, c.Rarity);
    }

    [Fact]
    public void Plan_HasRetain()
    {
        var c = new Plan();
        Assert.Contains(c.Keywords, k => k == CardKeyword.Retain);
    }

    [Fact]
    public void JustAsPlanned_IsBasicRarity()
    {
        var c = new JustAsPlanned();
        Assert.Equal(CardRarity.Basic, c.Rarity);
    }
}
