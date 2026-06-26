using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class PasticheTests
{
    [Fact]
    public void Pastiche_CardId_IsCorrect()
    {
        Assert.Equal("TheApprentice:Pastiche", Pastiche.CardId);
    }

    [Fact]
    public void Pastiche_IsSkill_Uncommon()
    {
        var c = new Pastiche();
        Assert.Equal(CardType.Skill, c.Type);
        Assert.Equal(CardRarity.Uncommon, c.Rarity);
    }
}
