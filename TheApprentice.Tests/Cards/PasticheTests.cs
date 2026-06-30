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

    [Fact]
    public void Pastiche_Costs1Energy()
    {
        var c = new Pastiche();
        Assert.Equal(1, c.EnergyCost.Canonical);
    }

    [Fact]
    public void Pastiche_HasNeitherExhaustNorExpend()
    {
        var c = new Pastiche();
        Assert.DoesNotContain(c.Keywords, k => k == CardKeyword.Exhaust);
        Assert.DoesNotContain(c.Keywords, k => k == ApprenticeKeywords.Expend);
    }
}
