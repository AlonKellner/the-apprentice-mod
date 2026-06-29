using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class DreamsAndAmbitionsTests
{
    [Fact] public void CountDreams_Empty_ReturnsZero() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([]));

    [Fact] public void CountDreams_OneDream_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountDreams([new Dream()]));

    [Fact] public void CountDreams_IgnoresAmbitions() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([new Ambition()]));

    [Fact] public void CountDreams_IgnoresPotentials() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([new Potential()]));

    [Fact] public void CountAmbitions_Empty_ReturnsZero() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAmbitions([]));

    [Fact] public void CountAmbitions_OneAmbition_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountAmbitions([new Ambition()]));

    [Fact] public void CountAmbitions_IgnoresDreams() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAmbitions([new Dream()]));

    [Fact] public void CountAll_MixedDreamsAndAmbitions_ReturnsCombinedCount() =>
        Assert.Equal(3, DreamsAndAmbitions.CountAll([new Dream(), new Ambition(), new Dream()]));

    [Fact] public void CountAll_IgnoresPotentials() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAll([new Potential()]));

    [Fact] public void CountPotentials_OnePotential_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountPotentials([new Potential()]));

    [Fact] public void CountPotentials_IgnoresDreams() =>
        Assert.Equal(0, DreamsAndAmbitions.CountPotentials([new Dream()]));

    // ── UpdateDreamyCards ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDreamyCards_SetsBlockToOriginalPlusTotal()
    {
        var card = new Dream();
        var mod = new DreamyModifier { OriginalBaseBlock = 5 };
        CardModifier.DirectModifiers(card).Add(mod);

        DreamsAndAmbitions.UpdateDreamyCards([card], 3);

        Assert.Equal(8m, card.DynamicVars.Block.BaseValue);
    }

    [Fact]
    public void UpdateDreamyCards_SetsAllCardsToSameAbsoluteValue()
    {
        var card1 = new Dream();
        var card2 = new Dream();
        var mod1 = new DreamyModifier { OriginalBaseBlock = 5 };
        var mod2 = new DreamyModifier { OriginalBaseBlock = 5 };
        CardModifier.DirectModifiers(card1).Add(mod1);
        CardModifier.DirectModifiers(card2).Add(mod2);

        DreamsAndAmbitions.UpdateDreamyCards([card1, card2], 3);

        Assert.Equal(8m, card1.DynamicVars.Block.BaseValue);
        Assert.Equal(8m, card2.DynamicVars.Block.BaseValue);
    }

    [Fact]
    public void UpdateDreamyCards_SecondCallOverridesFirstAbsolute()
    {
        var card = new Dream();
        var mod = new DreamyModifier { OriginalBaseBlock = 5 };
        CardModifier.DirectModifiers(card).Add(mod);

        DreamsAndAmbitions.UpdateDreamyCards([card], 2);
        DreamsAndAmbitions.UpdateDreamyCards([card], 5);

        Assert.Equal(10m, card.DynamicVars.Block.BaseValue); // 5+5, not 5+2+5
    }

    [Fact]
    public void UpdateDreamyCards_SkipsCardsWithoutDreamyModifier()
    {
        var dreamCard = new Dream();
        var otherCard = new Dream();
        var mod = new DreamyModifier { OriginalBaseBlock = 5 };
        CardModifier.DirectModifiers(dreamCard).Add(mod);
        // otherCard has no DreamyModifier

        DreamsAndAmbitions.UpdateDreamyCards([dreamCard, otherCard], 3);

        Assert.Equal(8m, dreamCard.DynamicVars.Block.BaseValue);
        Assert.Equal(Dream.BaseBlock, (int)otherCard.DynamicVars.Block.BaseValue); // unchanged
    }

    // ── UpdateAmbitousCards ──────────────────────────────────────────────────────

    [Fact]
    public void UpdateAmbitousCards_SetsDamageToOriginalPlusTotal()
    {
        var card = new Ambition();
        var mod = new AmbitousModifier { OriginalBaseDamage = 3 };
        CardModifier.DirectModifiers(card).Add(mod);

        DreamsAndAmbitions.UpdateAmbitousCards([card], 4);

        Assert.Equal(7m, card.DynamicVars.Damage.BaseValue);
    }

    [Fact]
    public void UpdateAmbitousCards_SetsAllCardsToSameAbsoluteValue()
    {
        var card1 = new Ambition();
        var card2 = new Ambition();
        var mod1 = new AmbitousModifier { OriginalBaseDamage = 3 };
        var mod2 = new AmbitousModifier { OriginalBaseDamage = 3 };
        CardModifier.DirectModifiers(card1).Add(mod1);
        CardModifier.DirectModifiers(card2).Add(mod2);

        DreamsAndAmbitions.UpdateAmbitousCards([card1, card2], 4);

        Assert.Equal(7m, card1.DynamicVars.Damage.BaseValue);
        Assert.Equal(7m, card2.DynamicVars.Damage.BaseValue);
    }

    [Fact]
    public void UpdateAmbitousCards_SecondCallOverridesFirstAbsolute()
    {
        var card = new Ambition();
        var mod = new AmbitousModifier { OriginalBaseDamage = 3 };
        CardModifier.DirectModifiers(card).Add(mod);

        DreamsAndAmbitions.UpdateAmbitousCards([card], 1);
        DreamsAndAmbitions.UpdateAmbitousCards([card], 4);

        Assert.Equal(7m, card.DynamicVars.Damage.BaseValue); // 3+4, not 3+1+4
    }
}
