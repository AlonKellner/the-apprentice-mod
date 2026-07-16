using System.Collections.Generic;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class StableModifierTests
{
#pragma warning disable STS001 // test-only stub — no localization entry needed
    private class PlainPower : UnderstudyCard
    {
        public PlainPower()
            : base(0, CardType.Power, CardRarity.Common, TargetType.None) { }
    }
#pragma warning restore STS001

    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheUnderstudy:Stable", StableModifier.ModifierId);
    }

    [Fact]
    public void CanApplyTo_AttackOrSkill_ReturnsTrue()
    {
        Assert.True(StableModifier.CanApplyTo(new UnderstudyStrike()));
        Assert.True(StableModifier.CanApplyTo(new UnderstudyDefend()));
    }

    [Fact]
    public void CanApplyTo_AlreadyStable_ReturnsFalse()
    {
        // Practice is printed-Stable via WithKeyword in its constructor.
        Assert.False(StableModifier.CanApplyTo(new Practice()));
    }

    [Fact]
    public void CanApplyTo_Power_ReturnsFalse()
    {
        Assert.False(StableModifier.CanApplyTo(new PlainPower()));
    }

    [Fact]
    public void Apply_IsStaticMethod()
    {
        // The generic AddModifier<T> call inside Apply requires a ModelDb lookup unavailable in
        // the bare test host (see project memory on test constraints) — behavior is covered via
        // manual in-game verification (Part B7 of the implementation plan) instead of a bare test.
        var method = typeof(StableModifier).GetMethod(
            "Apply", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void TryModifyKeywordsInCombat_AddsStableKeyword_WhenOwner()
    {
        var card = new UnderstudyStrike();
        var mod = new StableModifier();
        CardModifier.AddModifier(card, mod);
        var keywords = new HashSet<CardKeyword>();
        Assert.True(mod.TryModifyKeywordsInCombat(card, keywords));
        Assert.Contains(UnderstudyKeywords.Stable, keywords);
    }
}
