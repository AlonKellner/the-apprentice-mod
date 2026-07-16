using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Afflictions;

// Tests requiring CardCmd.Afflict<T>()/ModelDb cannot run without full game initialization
// (same documented boundary as CardModifier.AddModifier<T>) and are omitted; production
// attachment is verified in-game.
public class OrderTests
{
    [Fact]
    public void CanAfflictCardType_Attack_ReturnsTrue() =>
        Assert.True(new Order().CanAfflictCardType(CardType.Attack));

    [Fact]
    public void CanAfflictCardType_Skill_ReturnsTrue() =>
        Assert.True(new Order().CanAfflictCardType(CardType.Skill));

    [Fact]
    public void CanAfflictCardType_Power_ReturnsFalse() =>
        Assert.False(new Order().CanAfflictCardType(CardType.Power));

    [Fact]
    public void CanAfflict_UnderstudyStrike_ReturnsTrue() =>
        Assert.True(new Order().CanAfflict(new UnderstudyStrike()));

    [Fact]
    public void CanAfflict_StableSkill_ReturnsFalse() =>
        Assert.False(new Order().CanAfflict(new Practice()));

    [Fact]
    public void CanAfflict_RuntimeStableCard_ReturnsFalse()
    {
        // A card made Stable at runtime (via StableModifier, not just the printed keyword) must
        // be just as ineligible as a printed-Stable card like Practice.
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.False(new Order().CanAfflict(card));
    }

    [Fact]
    public void CanAfflict_PowerCard_ReturnsFalse() =>
        Assert.False(new Order().CanAfflict(new TheFirstLesson()));
}
