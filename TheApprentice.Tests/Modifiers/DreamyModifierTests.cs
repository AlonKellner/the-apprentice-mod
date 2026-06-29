using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// StoreSaveData/LoadSaveData require BaseLib.dll (not referenced in test project — verified in-game).
// TryModifyKeywordsInCombat is testable: use the instance overload CardModifier.AddModifier(card, mod)
// which calls ApplyInternal directly (no ModelDb needed). The generic AddModifier<T>() needs ModelDb.
public class DreamyModifierTests
{
    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheApprentice:Dreamy", DreamyModifier.ModifierId);
    }

    [Fact]
    public void OriginalBaseBlock_DefaultsToZero()
    {
        var mod = new DreamyModifier();
        Assert.Equal(0, mod.OriginalBaseBlock);
    }

    [Fact]
    public void OriginalBaseBlock_CanBeSet()
    {
        var mod = new DreamyModifier { OriginalBaseBlock = 7 };
        Assert.Equal(7, mod.OriginalBaseBlock);
    }

    // ── TryModifyKeywordsInCombat ────────────────────────────────────────────────

    [Fact]
    public void TryModifyKeywordsInCombat_WhenOwnerSetViaAddModifier_AddsDreamyKeyword()
    {
        var card = new Dream();
        var mod = new DreamyModifier();
        CardModifier.AddModifier(card, mod); // sets Owner = card

        var keywords = new HashSet<CardKeyword>();
        bool result = mod.TryModifyKeywordsInCombat(card, keywords);

        Assert.True(result);
        Assert.Contains(ApprenticeKeywords.Dreamy, keywords);
    }

    // Guards against regressing to DirectModifiers(card).Add(mod), which skips
    // ApplyInternal and leaves Owner null — causing TryModifyKeywordsInCombat to
    // silently return false and never show the keyword badge in-game.
    [Fact]
    public void TryModifyKeywordsInCombat_WhenOwnerNotSet_DoesNotAddKeyword()
    {
        var card = new Dream();
        var mod = new DreamyModifier();
        CardModifier.DirectModifiers(card).Add(mod); // Owner NOT set

        var keywords = new HashSet<CardKeyword>();
        bool result = mod.TryModifyKeywordsInCombat(card, keywords);

        Assert.False(result);
        Assert.DoesNotContain(ApprenticeKeywords.Dreamy, keywords);
    }
}
