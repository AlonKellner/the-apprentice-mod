using System.Reflection;
using BaseLib.Abstracts;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

public class IntenseModifierTests
{
    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheApprentice:Intense", IntenseModifier.ModifierId);
    }

    [Fact]
    public void CanApplyTo_AttackCardWithDamage_ReturnsTrue()
    {
        // ApprenticeStrike uses WithDamage → DamageVar with ValueProp.Move (powered).
        Assert.True(IntenseModifier.CanApplyTo(new ApprenticeStrike()));
    }

    [Fact]
    public void CanApplyTo_SkillCardWithBlock_ReturnsTrue()
    {
        // ApprenticeDefend uses WithBlock → BlockVar with ValueProp.Move (powered).
        Assert.True(IntenseModifier.CanApplyTo(new ApprenticeDefend()));
    }

    [Fact]
    public void CanApplyTo_StrikeB_ReturnsTrue()
    {
        Assert.True(IntenseModifier.CanApplyTo(new StrikeB()));
    }

    [Fact]
    public void CanApplyTo_DefendB_ReturnsTrue()
    {
        Assert.True(IntenseModifier.CanApplyTo(new DefendB()));
    }

    [Fact]
    public void CanApplyTo_Performance_ReturnsFalse()
    {
        // Performance has no Damage or Block DynamicVar — Intense cannot benefit it.
        Assert.False(IntenseModifier.CanApplyTo(new Performance()));
    }

    [Fact]
    public void CanApplyTo_Intention_ReturnsFalse()
    {
        // Intention has no Damage or Block DynamicVar — Intense cannot benefit it.
        Assert.False(IntenseModifier.CanApplyTo(new Intention()));
    }

    [Fact]
    public void CanApplyTo_AlreadyIntenseCard_ReturnsTrue()
    {
        // Intense stacks: re-applying to a card that already has Intense adds another stack.
        var card = new ApprenticeStrike();
        var mod = new IntenseModifier();
        CardModifier.AddModifier(card, mod);
        Assert.True(IntenseModifier.CanApplyTo(card));
    }

    [Fact]
    public void CanApplyTo_IsStaticMethod()
    {
        var method = typeof(IntenseModifier).GetMethod(
            "CanApplyTo", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Apply_IsStaticMethod()
    {
        var method = typeof(IntenseModifier).GetMethod(
            "Apply", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void IntenseField_ExistsOnApprenticeKeywords()
    {
        var field = typeof(ApprenticeKeywords).GetField("Intense");
        Assert.NotNull(field);
    }

    [Fact]
    public void Stacks_DefaultsToZero()
    {
        var mod = new IntenseModifier();
        Assert.Equal(0, mod.Stacks);
    }

    [Fact]
    public void IsSpent_DefaultsToFalse()
    {
        var mod = new IntenseModifier();
        Assert.False(mod.IsSpent);
    }

    [Fact]
    public void ModifyDamageAdditive_IsVirtualMethod()
    {
        // IntenseModifier provides the Strength-style damage bonus hook.
        var method = typeof(IntenseModifier).GetMethod("ModifyDamageAdditive");
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(IntenseModifier));
    }

    [Fact]
    public void ModifyBlockAdditive_IsVirtualMethod()
    {
        // IntenseModifier provides the Dexterity-style block bonus hook.
        var method = typeof(IntenseModifier).GetMethod("ModifyBlockAdditive");
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(IntenseModifier));
    }

    [Fact]
    public void ModifyDescription_ShowsStackCount_BeforeDescription()
    {
        var mod = new IntenseModifier();
        typeof(IntenseModifier).GetProperty(nameof(IntenseModifier.Stacks))!.SetValue(mod, 2);
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.StartsWith("[gold]Intense 2[/gold].", description);
        Assert.Contains("Deal 6 damage.", description);
    }

    [Fact]
    public void ModifyDescription_NoStacks_DoesNotModify()
    {
        var mod = new IntenseModifier();
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.Equal("Deal 6 damage.", description);
    }
}
