using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

// Guiding principle: a card's tooltip must show exactly what its description says.
// If the description says "Ambition", the tip shows Ambition (unupgraded).
// If the description says "Ambition+" or "Potential+", the tip shows the upgraded version.
//
// typeof(T) implicit conversion in BaseLib always produces an unupgraded tip (upgrade=false).
// The typeof(T) operator captures a local variable `t2` of type System.Type in a closure.
// Explicit lambdas (card => FromCard<T>(upgrade:...)) may also compile as closures, but their
// closure class will NOT contain a System.Type field — only the typeof(T) conversion does.
// So: we flag a tip as implicit-conversion if its closure captures a System.Type value.
public class TooltipConsistencyTests
{
    private static List<TooltipSource> GetHoverTips(ConstructedCardModel card)
    {
        var field = typeof(ConstructedCardModel)
            .GetField("_hoverTips", BindingFlags.NonPublic | BindingFlags.Instance);
        return (field?.GetValue(card) as List<TooltipSource>) ?? new List<TooltipSource>();
    }

    private static bool UsesImplicitTypeConversion(TooltipSource tip)
    {
        var makeTipField = typeof(TooltipSource)
            .GetField("_makeTip", BindingFlags.NonPublic | BindingFlags.Instance);
        var makeTip = makeTipField?.GetValue(tip) as Delegate;
        if (makeTip == null) return false;
        if (makeTip.Target == null) return false; // static delegate — definitely not a typeof(T) closure
        // The typeof(T) implicit operator captures `t2: System.Type`. No other tip pattern does.
        return makeTip.Target.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Any(f => f.FieldType == typeof(Type));
    }

    private static Type? GetCapturedType(TooltipSource tip)
    {
        var makeTipField = typeof(TooltipSource)
            .GetField("_makeTip", BindingFlags.NonPublic | BindingFlags.Instance);
        var makeTip = makeTipField?.GetValue(tip) as Delegate;
        if (makeTip?.Target == null) return null;
        var typeField = makeTip.Target.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(Type));
        return typeField?.GetValue(makeTip.Target) as Type;
    }

    private static Dictionary<string, string> LoadCardsJson()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var path = Path.Combine(root, "../TheApprentice/localization/eng/cards.json");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
    }

    private static string ToLocKeyPrefix(Type t) =>
        "THEAPPRENTICE-" + Regex.Replace(t.Name, @"([a-z])([A-Z])", "$1_$2").ToUpperInvariant();

    // Fails before fix (Pastiche uses typeof(Potential) which always shows unupgraded).
    // Passes after fix (explicit lambda with upgrade: true).
    [Fact]
    public void Pastiche_PotentialTip_DoesNotUseImplicitConversion()
    {
        var tips = GetHoverTips(new Pastiche());
        Assert.All(tips, tip =>
            Assert.False(UsesImplicitTypeConversion(tip),
                "Pastiche description says 'Potential+' but tip uses typeof(Potential) " +
                "which always shows unupgraded. Use explicit TooltipSource instead."));
    }

    // Regression guard — Sublimation was already fixed; this should always pass.
    [Fact]
    public void Sublimation_AmbitionTip_DoesNotUseImplicitConversion()
    {
        var tips = GetHoverTips(new Sublimation());
        Assert.All(tips, tip =>
            Assert.False(UsesImplicitTypeConversion(tip)));
    }

    // Parameterized: any card whose description references an upgraded token
    // must not use the implicit typeof(T) conversion for that tip.
    [Theory]
    [InlineData(typeof(Pastiche))]    // description: "[gold]Potential+[/gold]"
    [InlineData(typeof(Sublimation))] // description: {AmbitionType} -> "Ambition+" when upgraded
    public void Card_WithUpgradedTokenInDescription_HasAtLeastOneExplicitLambdaTip(Type cardType)
    {
        var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
        var tips = GetHoverTips(card);
        Assert.NotEmpty(tips);
        Assert.Contains(tips, tip => !UsesImplicitTypeConversion(tip));
    }

    // Comprehensive: scans ALL cards in the assembly. For each typeof(T) tip, reads the
    // card's localization descriptions. Fails if the description says "TokenName+" but the
    // tip uses typeof(TokenName) which can never show the upgraded version.
    [Fact]
    public void AllCards_ImplicitTypeTips_NeverUsedWhenDescriptionMentionsUpgradedToken()
    {
        var json = LoadCardsJson();
        var violations = new List<string>();

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var locPrefix = ToLocKeyPrefix(cardType);

            foreach (var tip in GetHoverTips(card).Where(UsesImplicitTypeConversion))
            {
                var capturedType = GetCapturedType(tip);
                if (capturedType == null) continue;
                var tokenName = capturedType.Name; // "Ambition", "Dream", "Potential"

                json.TryGetValue($"{locPrefix}.description",  out var baseDesc);
                json.TryGetValue($"{locPrefix}+.description", out var upgDesc);

                if (baseDesc?.Contains(tokenName + "+") == true ||
                    upgDesc?.Contains(tokenName + "+") == true)
                    violations.Add(
                        $"{cardType.Name}: typeof({tokenName}) tip used but description says '{tokenName}+'");
            }
        }

        Assert.Empty(violations);
    }
}
