using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
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
    public void CanApplyTo_StableSkill_ReturnsFalse() => Assert.False(OrderModifier.CanApplyTo(new WarmUp()));

    [Fact]
    public void CanApplyTo_RuntimeStableCard_ReturnsFalse()
    {
        // A card made Stable at runtime (via StableModifier, not just the printed keyword) must
        // be just as ineligible as a printed-Stable card like WarmUp.
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.False(OrderModifier.CanApplyTo(card));
    }

    // ModifyDescriptionPost — prepends (not appends) the Order line as the card's first line.

    [Fact]
    public void ModifyDescriptionPost_PlayThis_PrependsExactLine()
    {
        var mod = new OrderModifier { OrderKind = OrderModifier.Kind.PlayThis };
        var args = new object?[] { null, "base description" };
        typeof(OrderModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Equal("[red][sine]Play this card.[/sine][/red]\nbase description", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_DontPlayThis_PrependsExactLine()
    {
        var mod = new OrderModifier { OrderKind = OrderModifier.Kind.DontPlayThis };
        var args = new object?[] { null, "base description" };
        typeof(OrderModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Equal("[red][sine]Don't play this card.[/sine][/red]\nbase description", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_FlavorOnly_PrependsFlavorText()
    {
        var mod = new OrderModifier { OrderKind = OrderModifier.Kind.FlavorOnly, FlavorText = "Stand still." };
        var args = new object?[] { null, "base description" };
        typeof(OrderModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Equal("[red][sine]Stand still.[/sine][/red]\nbase description", (string)args[1]!);
    }

    [Fact]
    public void FlavorLines_HasExpectedCount() => Assert.Equal(14, OrderModifier.FlavorLines.Length);

    [Fact]
    public void WasPlayed_DefaultsToFalse() => Assert.False(new OrderModifier().WasPlayed);
}
