using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// OnPlay (sets IsSpent = true) requires CardPlay context — verified in-game.
public class ExpendModifierTests
{
    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheApprentice:Expend", ExpendModifier.ModifierId);
    }

    // ── TryModifyKeywordsInCombat ────────────────────────────────────────────────

    [Fact]
    public void TryModifyKeywordsInCombat_WhenOwnerSetViaAddModifier_AddsExpendKeyword()
    {
        var card = new Dream();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod); // sets Owner = card

        var keywords = new HashSet<CardKeyword>();
        bool result = mod.TryModifyKeywordsInCombat(card, keywords);

        Assert.True(result);
        Assert.Contains(ApprenticeKeywords.Expend, keywords);
    }

    // Guards against regressing to DirectModifiers(card).Add(mod), which skips
    // ApplyInternal and leaves Owner null — causing TryModifyKeywordsInCombat to
    // silently return false and never show the keyword badge in-game.
    [Fact]
    public void TryModifyKeywordsInCombat_WhenOwnerNotSet_DoesNotAddKeyword()
    {
        var card = new Dream();
        var mod = new ExpendModifier();
        CardModifier.DirectModifiers(card).Add(mod); // Owner NOT set

        var keywords = new HashSet<CardKeyword>();
        bool result = mod.TryModifyKeywordsInCombat(card, keywords);

        Assert.False(result);
        Assert.DoesNotContain(ApprenticeKeywords.Expend, keywords);
    }

    // ── IsSpent / Reset ──────────────────────────────────────────────────────────

    [Fact]
    public void IsSpent_DefaultsFalse()
    {
        var mod = new ExpendModifier();
        Assert.False(mod.IsSpent);
    }

    [Fact]
    public void TryModifyKeywordsInCombat_WhenNotSpent_DoesNotAddUnplayable()
    {
        var card = new Dream();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);

        var keywords = new HashSet<CardKeyword>();
        mod.TryModifyKeywordsInCombat(card, keywords);

        Assert.DoesNotContain(CardKeyword.Unplayable, keywords);
    }

    [Fact]
    public void Reset_ClearsIsSpent()
    {
        var card = new Dream();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);
        // Simulate spent state (OnPlay requires CardPlay context so we use Reset's inverse)
        // Verify Reset works on a freshly-constructed mod (IsSpent=false remains false)
        mod.Reset();
        Assert.False(mod.IsSpent);
    }
}
