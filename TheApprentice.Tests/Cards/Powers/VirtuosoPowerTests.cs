using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using Xunit;

namespace TheApprentice.Tests.Cards.Powers;

// Tests that need a Planned card (requiring CardModifier.AddModifier → ModelDb) are
// omitted since ModelDb requires full game initialization. The keyword suppression
// behavior of VirtuosoPower is verified in-game.
public class VirtuosoPowerTests
{
    [Fact]
    public void TryModifyKeywordsInCombat_NonPlannedCard_ReturnsFalse()
    {
        // A card with no PlannedModifier — VirtuosoPower should leave keywords unchanged
        var card = new TheApprentice.TheApprenticeCode.Cards.ApprenticeStrike();
        var keywords = new HashSet<CardKeyword> { CardKeyword.Unplayable };

        var power = new VirtuosoPower();
        bool result = power.TryModifyKeywordsInCombat(card, keywords);

        Assert.False(result);
        Assert.Contains(CardKeyword.Unplayable, keywords);
    }
}
