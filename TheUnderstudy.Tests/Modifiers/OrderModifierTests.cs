using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

// Tests requiring CardModifier.AddModifier<T>() (which queries ModelDb) cannot run without full
// game initialization and are omitted, mirroring PlannedModifierTests.cs's own documented boundary.
// The production attach path (SecondLessonPower assigning Orders) is verified in-game.
public class OrderModifierTests
{
    // OnCardPlayed — resolution the instant the order-carrying card is played.

    [Fact]
    public void OnCardPlayed_PlayThis_IsReward() =>
        Assert.Equal(OrderModifier.Resolution.Reward, OrderModifier.OnCardPlayed(OrderModifier.Kind.PlayThis));

    [Fact]
    public void OnCardPlayed_DontPlayThis_IsPunish() =>
        Assert.Equal(OrderModifier.Resolution.Punish, OrderModifier.OnCardPlayed(OrderModifier.Kind.DontPlayThis));

    [Fact]
    public void OnCardPlayed_FlavorOnly_IsNone() =>
        Assert.Equal(OrderModifier.Resolution.None, OrderModifier.OnCardPlayed(OrderModifier.Kind.FlavorOnly));

    // OnTurnEndIfUnresolved — mirror resolution for an order never triggered by play.

    [Fact]
    public void OnTurnEndIfUnresolved_PlayThis_IsPunish() =>
        Assert.Equal(OrderModifier.Resolution.Punish, OrderModifier.OnTurnEndIfUnresolved(OrderModifier.Kind.PlayThis));

    [Fact]
    public void OnTurnEndIfUnresolved_DontPlayThis_IsReward() =>
        Assert.Equal(OrderModifier.Resolution.Reward, OrderModifier.OnTurnEndIfUnresolved(OrderModifier.Kind.DontPlayThis));

    [Fact]
    public void OnTurnEndIfUnresolved_FlavorOnly_IsNone() =>
        Assert.Equal(OrderModifier.Resolution.None, OrderModifier.OnTurnEndIfUnresolved(OrderModifier.Kind.FlavorOnly));

    // CanApplyTo — Attack/Skill only, not Stable-tagged. Uses real existing cards, matching the
    // established idiom in PlannedModifierTests.cs/TunedModifierTests.cs.

    [Fact]
    public void CanApplyTo_UnderstudyStrike_ReturnsTrue() => Assert.True(OrderModifier.CanApplyTo(new UnderstudyStrike()));

    [Fact]
    public void CanApplyTo_UnderstudyDefend_ReturnsTrue() => Assert.True(OrderModifier.CanApplyTo(new UnderstudyDefend()));

    [Fact]
    public void CanApplyTo_PowerCard_ReturnsFalse() => Assert.False(OrderModifier.CanApplyTo(new TheFirstLesson()));

    [Fact]
    public void CanApplyTo_StableSkill_ReturnsFalse() => Assert.False(OrderModifier.CanApplyTo(new Practice()));

    [Fact]
    public void CanApplyTo_AlreadyAfflictedCard_ReturnsFalse()
    {
        // Two Lessons may not both order the same card. Whichever assigns first afflicts it with
        // Order, and that affliction is what takes the card out of the other Lesson's candidate pool.
        var card = new UnderstudyStrike();
        GiveAffliction(card, new Order());
        Assert.False(OrderModifier.CanApplyTo(card));
    }

    // CardCmd.Afflict and CardModel.AfflictInternal both need a mutable card, and MutableClone()
    // reaches into ModelDb — neither is available in the bare test host (see the file header). Drive
    // the private setter directly instead: Affliction is precisely the state CanApplyTo reads.
    private static void GiveAffliction(CardModel card, AfflictionModel affliction) =>
        typeof(CardModel).GetProperty(nameof(CardModel.Affliction))!
            .GetSetMethod(nonPublic: true)!.Invoke(card, new object?[] { affliction });

    [Fact]
    public void CanApplyTo_RuntimeStableCard_ReturnsFalse()
    {
        // A card made Stable at runtime (via StableModifier, not just the printed keyword) must
        // be just as ineligible as a printed-Stable card like Practice.
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.False(OrderModifier.CanApplyTo(card));
    }

    // Decorate — the styling half of ModifyDescriptionPost, prepending (not appending) the Order as
    // the card's first line. The words now come from the loc table, which the bare test host cannot
    // load, so the pure formatting seam is what is asserted here; the text itself is checked against
    // the shipping JSON in LocFileTests.

    [Fact]
    public void Decorate_PrependsOrderAsFirstLine() =>
        Assert.Equal("[red][sine]Play this card.[/sine][/red]\nbase description",
            OrderModifier.Decorate("Play this card.", "base description"));

    [Fact]
    public void Decorate_EmptyText_LeavesDescriptionUntouched() =>
        Assert.Equal("base description", OrderModifier.Decorate("", "base description"));

    [Fact]
    public void WasPlayed_DefaultsToFalse() => Assert.False(new OrderModifier().WasPlayed);
}
