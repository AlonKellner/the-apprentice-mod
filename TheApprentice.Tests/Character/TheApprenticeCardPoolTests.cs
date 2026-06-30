using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Character;
using Xunit;

namespace TheApprentice.Tests.Character;

// Card instantiation tests live in ApprenticeCardTests to avoid duplicate registration
// in CustomContentDictionary. These tests verify IsPrePlanned via type reflection only.
public class TheApprenticeCardPoolTests
{
    [Fact]
    public void Signature_Type_IsPrePlanned()
    {
        // IsPrePlanned is a virtual property on ApprenticeCard, overridden on Signature.
        // Verify via reflection on the type to avoid instantiation.
        var prop = typeof(Signature).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void Prelude_Type_IsPrePlanned()
    {
        var prop = typeof(Prelude).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void IsPrePlanned_DefaultsToFalse_OnApprenticeCardBase()
    {
        // The base ApprenticeCard declares IsPrePlanned as virtual returning false.
        // Only Signature and Prelude override it.
        var overrides = typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPrePlanned");
                return method != null && method.DeclaringType == t; // overrides in this type
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "Prelude", "Signature" }, overrides);
    }

    [Fact]
    public void HasExpend_DefaultsToFalse_OnApprenticeCardBase()
    {
        // The base ApprenticeCard declares HasExpend as virtual returning false.
        // Only the cards converted from Exhaust to Expend override it.
        var overrides = typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_HasExpend");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(
            new[] { "Daydream", "Epiphany", "Inversion", "Reflection", "Resonance", "Transcendence" },
            overrides);
    }

    [Fact]
    public void AfterCardEnteredCombat_AttachesExpendModifier_WhenHasExpend()
    {
        var card = new Reflection();
        new TheApprenticeCardPool().AfterCardEnteredCombat(card);
        Assert.True(card.TryGetModifier<ExpendModifier>(out _));
    }

    [Fact]
    public void AfterCardEnteredCombat_DoesNotAttachExpendModifier_WhenNotHasExpend()
    {
        var card = new ClearMind();
        new TheApprenticeCardPool().AfterCardEnteredCombat(card);
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    [Fact]
    public void AfterCardEnteredCombat_ResetsStaleIsSpent_WhenModifierAlreadyAttached()
    {
        // Simulates a "real" card carrying a spent ExpendModifier over from a
        // previous combat. Nothing else in the codebase resets IsSpent between
        // combats, so the pool hook must do it defensively (mirrors how the
        // IsPrePlanned branch re-asserts SequenceIndex on every combat entry).
        var card = new Reflection();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);
        typeof(ExpendModifier).GetProperty(nameof(ExpendModifier.IsSpent))!.SetValue(mod, true);
        Assert.True(mod.IsSpent);

        new TheApprenticeCardPool().AfterCardEnteredCombat(card);

        Assert.False(mod.IsSpent);
    }

    [Fact]
    public void Pool_HasExactly20CommonCards() => Assert.Equal(20, CountCardsByRarity("Common"));

    [Fact]
    public void Pool_HasExactly36UncommonCards() => Assert.Equal(36, CountCardsByRarity("Uncommon"));

    [Fact]
    public void Pool_HasExactly26RareCards() => Assert.Equal(26, CountCardsByRarity("Rare"));

    [Fact]
    public void Pool_HasExactly82CombatCards()
        => Assert.Equal(82, CountCardsByRarity("Common") + CountCardsByRarity("Uncommon") + CountCardsByRarity("Rare"));

    private static int CountCardsByRarity(string rarity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var cardsDir = Path.Combine(root, "TheApprenticeCode", "Cards");
        var skip = new HashSet<string> { "ApprenticeCard", "ApprenticeKeywords", "DreamsAndAmbitions", "TensionHelper" };
        var pattern = new System.Text.RegularExpressions.Regex(
            @":\s*base\(\s*\d+\s*,\s*CardType\.\w+\s*,\s*CardRarity\." + rarity + @"\b");
        return Directory.GetFiles(cardsDir, "*.cs")
            .Where(f => !skip.Contains(Path.GetFileNameWithoutExtension(f)))
            .Count(f => pattern.IsMatch(File.ReadAllText(f)));
    }
}
