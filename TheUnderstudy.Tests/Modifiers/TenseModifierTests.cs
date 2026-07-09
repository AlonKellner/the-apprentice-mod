using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class TenseModifierTests
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
        Assert.Equal("TheUnderstudy:Tense", TenseModifier.ModifierId);
    }

    [Fact]
    public void CanApplyTo_UnderstudyStrike_ReturnsTrue()
    {
        // UnderstudyStrike uses WithDamage → DamageVar with ValueProp.Move (powered).
        Assert.True(TenseModifier.CanApplyTo(new UnderstudyStrike()));
    }

    [Fact]
    public void CanApplyTo_UnderstudyDefend_ReturnsTrue()
    {
        // UnderstudyDefend uses WithBlock → BlockVar with ValueProp.Move (powered).
        Assert.True(TenseModifier.CanApplyTo(new UnderstudyDefend()));
    }

    [Fact]
    public void CanApplyTo_Performance_ReturnsFalse()
    {
        // Performance has no Damage or Block DynamicVar — Tense cannot benefit it.
        Assert.False(TenseModifier.CanApplyTo(new Performance()));
    }

    [Fact]
    public void CanApplyTo_Buildup_ReturnsFalse()
    {
        // Buildup has no Damage or Block DynamicVar — Tense cannot benefit it.
        Assert.False(TenseModifier.CanApplyTo(new Buildup()));
    }

    [Fact]
    public void CanApplyTo_EverythingIveGot_ReturnsTrueAfterGeneralization()
    {
        // EverythingIveGot is a non-Stable Skill with no Damage/Block DynamicVar. Tense now
        // applies to any Attack/Skill (matching PlannedModifier/UnplayableModifier's own
        // eligibility check), not just cards that print damage or block — a pure-utility Skill
        // still gets the "becomes Unplayable when played" behavior, just no numeric bonus.
        Assert.True(TenseModifier.CanApplyTo(new EverythingIveGot()));
    }

    [Fact]
    public void CanApplyTo_AlreadyTenseCard_ReturnsTrue()
    {
        // Tense stacks: re-applying to a card that already has Tense adds another stack.
        var card = new UnderstudyStrike();
        var mod = new TenseModifier();
        CardModifier.AddModifier(card, mod);
        Assert.True(TenseModifier.CanApplyTo(card));
    }

    [Fact]
    public void CanApplyTo_IsStaticMethod()
    {
        var method = typeof(TenseModifier).GetMethod(
            "CanApplyTo", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Apply_IsStaticMethod()
    {
        var method = typeof(TenseModifier).GetMethod(
            "Apply", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void TenseField_ExistsOnUnderstudyKeywords()
    {
        var field = typeof(UnderstudyKeywords).GetField("Tense");
        Assert.NotNull(field);
    }

    [Fact]
    public void Stacks_DefaultsToZero()
    {
        var mod = new TenseModifier();
        Assert.Equal(0, mod.Stacks);
    }

    [Fact]
    public void IsFinalTensePlay_NoTenseModifier_SinglePlay_ReturnsFalse()
    {
        var card = new UnderstudyStrike();
        Assert.False(TenseModifier.IsFinalTensePlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalTensePlay_HasTense_SinglePlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TenseModifier());
        Assert.True(TenseModifier.IsFinalTensePlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalTensePlay_HasTense_Replay1_FirstPlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TenseModifier());
        // Replay 1 -> PlayCount = 2; first play is PlayIndex 0.
        Assert.False(TenseModifier.IsFinalTensePlay(MakePlay(card, 0, 2)));
    }

    [Fact]
    public void IsFinalTensePlay_HasTense_Replay1_SecondPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TenseModifier());
        Assert.True(TenseModifier.IsFinalTensePlay(MakePlay(card, 1, 2)));
    }

    [Fact]
    public void IsFinalTensePlay_HasTense_Replay2_MiddlePlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TenseModifier());
        // Replay 2 -> PlayCount = 3; middle play is PlayIndex 1.
        Assert.False(TenseModifier.IsFinalTensePlay(MakePlay(card, 1, 3)));
    }

    [Fact]
    public void IsFinalTensePlay_HasTense_Replay2_FinalPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TenseModifier());
        Assert.True(TenseModifier.IsFinalTensePlay(MakePlay(card, 2, 3)));
    }

    [Fact]
    public void IsFinalTensePlay_GrantedAfterOwnCheckThisSamePlay_ReturnsFalse()
    {
        // Da Capo's case: Tense was granted after this exact play's own attack, so it
        // shouldn't lock the card up for THIS play.
        var card = new UnderstudyStrike();
        var mod = new TenseModifier();
        CardModifier.AddModifier(card, mod);
        var play = MakePlay(card, 0, 1);
        typeof(TenseModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, play);
        Assert.False(TenseModifier.IsFinalTensePlay(play));
    }

    [Fact]
    public void IsFinalTensePlay_GrantedAfterOwnCheckOnAnEarlierPlay_ReturnsTrue()
    {
        // The next time the card is played, Tense was already active before this new play's
        // own check ran, so the normal locking rule applies again.
        var card = new UnderstudyStrike();
        var mod = new TenseModifier();
        CardModifier.AddModifier(card, mod);
        var firstPlay = MakePlay(card, 0, 1);
        typeof(TenseModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, firstPlay);
        var secondPlay = MakePlay(card, 0, 1);
        Assert.True(TenseModifier.IsFinalTensePlay(secondPlay));
    }

    [Fact]
    public void ModifyDamageAdditive_IsVirtualMethod()
    {
        // TenseModifier provides the Strength-style damage bonus hook.
        var method = typeof(TenseModifier).GetMethod("ModifyDamageAdditive");
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(TenseModifier));
    }

    [Fact]
    public void ModifyBlockAdditive_IsVirtualMethod()
    {
        // TenseModifier provides the Dexterity-style block bonus hook.
        var method = typeof(TenseModifier).GetMethod("ModifyBlockAdditive");
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(TenseModifier));
    }

    [Fact]
    public void DoubleStacks_StacksOne_BecomesTwo()
    {
        var card = new UnderstudyStrike();
        var mod = new TenseModifier();
        CardModifier.AddModifier(card, mod);
        typeof(TenseModifier).GetProperty(nameof(TenseModifier.Stacks))!.SetValue(mod, 1);
        TenseModifier.DoubleStacks(card);
        Assert.Equal(2, mod.Stacks);
    }

    [Fact]
    public void DoubleStacks_StacksThree_BecomesSix()
    {
        var card = new UnderstudyStrike();
        var mod = new TenseModifier();
        CardModifier.AddModifier(card, mod);
        typeof(TenseModifier).GetProperty(nameof(TenseModifier.Stacks))!.SetValue(mod, 3);
        TenseModifier.DoubleStacks(card);
        Assert.Equal(6, mod.Stacks);
    }

    [Fact]
    public void DoubleStacks_NoTenseModifier_NoOp()
    {
        var card = new UnderstudyStrike();
        TenseModifier.DoubleStacks(card); // must not throw
        Assert.False(card.TryGetModifier<TenseModifier>(out _));
    }

    [Fact]
    public void ModifyDescription_ShowsStackCount_BeforeDescription()
    {
        var mod = new TenseModifier();
        typeof(TenseModifier).GetProperty(nameof(TenseModifier.Stacks))!.SetValue(mod, 2);
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.StartsWith("[gold]Tense 2[/gold].", description);
        Assert.Contains("Deal 6 damage.", description);
    }

    [Fact]
    public void ModifyDescription_NoStacks_DoesNotModify()
    {
        var mod = new TenseModifier();
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.Equal("Deal 6 damage.", description);
    }
}
