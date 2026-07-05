using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class IntenseModifierTests
{
    private static CardPlay MakePlay(CardModel card, int index, int count) => new()
    {
        Card = card,
        Target = null,
        ResultPile = PileType.Discard,
        Resources = default,
        IsAutoPlay = false,
        PlayIndex = index,
        PlayCount = count,
    };

    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheUnderstudy:Intense", IntenseModifier.ModifierId);
    }

    [Fact]
    public void CanApplyTo_UnderstudyStrike_ReturnsTrue()
    {
        // UnderstudyStrike uses WithDamage → DamageVar with ValueProp.Move (powered).
        Assert.True(IntenseModifier.CanApplyTo(new UnderstudyStrike()));
    }

    [Fact]
    public void CanApplyTo_UnderstudyDefend_ReturnsTrue()
    {
        // UnderstudyDefend uses WithBlock → BlockVar with ValueProp.Move (powered).
        Assert.True(IntenseModifier.CanApplyTo(new UnderstudyDefend()));
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
        var card = new UnderstudyStrike();
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
    public void IntenseField_ExistsOnUnderstudyKeywords()
    {
        var field = typeof(UnderstudyKeywords).GetField("Intense");
        Assert.NotNull(field);
    }

    [Fact]
    public void Stacks_DefaultsToZero()
    {
        var mod = new IntenseModifier();
        Assert.Equal(0, mod.Stacks);
    }

    [Fact]
    public void IsFinalIntensePlay_NoIntenseModifier_SinglePlay_ReturnsFalse()
    {
        var card = new UnderstudyStrike();
        Assert.False(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalIntensePlay_HasIntense_SinglePlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new IntenseModifier());
        Assert.True(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalIntensePlay_HasIntense_Replay1_FirstPlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new IntenseModifier());
        // Replay 1 -> PlayCount = 2; first play is PlayIndex 0.
        Assert.False(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 0, 2)));
    }

    [Fact]
    public void IsFinalIntensePlay_HasIntense_Replay1_SecondPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new IntenseModifier());
        Assert.True(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 1, 2)));
    }

    [Fact]
    public void IsFinalIntensePlay_HasIntense_Replay2_MiddlePlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new IntenseModifier());
        // Replay 2 -> PlayCount = 3; middle play is PlayIndex 1.
        Assert.False(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 1, 3)));
    }

    [Fact]
    public void IsFinalIntensePlay_HasIntense_Replay2_FinalPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new IntenseModifier());
        Assert.True(IntenseModifier.IsFinalIntensePlay(MakePlay(card, 2, 3)));
    }

    [Fact]
    public void IsFinalIntensePlay_GrantedAfterOwnCheckThisSamePlay_ReturnsFalse()
    {
        // Da Capo's case: Intense was granted after this exact play's own attack, so it
        // shouldn't lock the card up for THIS play.
        var card = new UnderstudyStrike();
        var mod = new IntenseModifier();
        CardModifier.AddModifier(card, mod);
        var play = MakePlay(card, 0, 1);
        typeof(IntenseModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, play);
        Assert.False(IntenseModifier.IsFinalIntensePlay(play));
    }

    [Fact]
    public void IsFinalIntensePlay_GrantedAfterOwnCheckOnAnEarlierPlay_ReturnsTrue()
    {
        // The next time the card is played, Intense was already active before this new play's
        // own check ran, so the normal locking rule applies again.
        var card = new UnderstudyStrike();
        var mod = new IntenseModifier();
        CardModifier.AddModifier(card, mod);
        var firstPlay = MakePlay(card, 0, 1);
        typeof(IntenseModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, firstPlay);
        var secondPlay = MakePlay(card, 0, 1);
        Assert.True(IntenseModifier.IsFinalIntensePlay(secondPlay));
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
