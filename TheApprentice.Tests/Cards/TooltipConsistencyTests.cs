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
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Powers;
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

    private static bool HasTipForType(List<TooltipSource> tips, Type powerType) =>
        tips.Any(tip => GetCapturedType(tip) == powerType);

    // Maps gold-tagged keyword names in descriptions to the PowerModel type that should provide the tooltip.
    private static readonly Dictionary<string, Type> PowerKeywordToType = new()
    {
        { "Weak",          typeof(WeakPower) },
        { "Vulnerable",    typeof(VulnerablePower) },
        { "Strength",      typeof(StrengthPower) },
        { "Unweak",        typeof(UnweakPower) },
        { "Unvulnerable",  typeof(UnvulnerablePower) },
    };

    // Forward direction: if the BASE description mentions [gold]X[/gold], the card must have an
    // always-present typeof(T) tip for X's PowerModel.
    //
    // Upgrade-only keywords (appearing only in +.description) are expected to be covered by
    // conditional lambda tips — those are not detectable by GetCapturedType reflection and are
    // intentionally excluded from this automated check.
    [Fact]
    public void AllCards_PowerKeywordsInDescription_HaveMatchingHoverTips()
    {
        var json = LoadCardsJson();
        var violations = new List<string>();

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var tips = GetHoverTips(card);
            var locPrefix = ToLocKeyPrefix(cardType);

            json.TryGetValue($"{locPrefix}.description", out var baseDesc);

            foreach (var (keyword, powerType) in PowerKeywordToType)
            {
                var tag = $"[gold]{keyword}[/gold]";
                if (baseDesc?.Contains(tag) != true) continue;

                if (!HasTipForType(tips, powerType))
                    violations.Add($"{cardType.Name}: base description mentions '{keyword}' but has no {powerType.Name} hover tip");
            }
        }

        Assert.Empty(violations);
    }

    // Reverse direction: if a card has a typeof(T) power tip in its constructor, its keyword must
    // appear in the BASE description. Constructor tips show on the unupgraded card, so their
    // keywords must be explained in the base description — not just the upgrade description.
    //
    // Upgrade-only tips must be added in OnUpgrade() so this test won't flag them.
    // Guards against dangling tooltips like a VulnerablePower tip when the base description
    // only mentions Weak.
    [Fact]
    public void AllCards_PowerHoverTips_OnlyAddedWhenKeywordAppearsInDescription()
    {
        var json = LoadCardsJson();
        var violations = new List<string>();

        // Invert the map: PowerModel type → keyword string
        var typeToKeyword = PowerKeywordToType.ToDictionary(kv => kv.Value, kv => kv.Key);

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var tips = GetHoverTips(card);
            var locPrefix = ToLocKeyPrefix(cardType);

            json.TryGetValue($"{locPrefix}.description", out var baseDesc);

            foreach (var tip in tips)
            {
                var capturedType = GetCapturedType(tip);
                if (capturedType == null || !typeToKeyword.TryGetValue(capturedType, out var keyword)) continue;

                var tag = $"[gold]{keyword}[/gold]";
                if (baseDesc?.Contains(tag) != true)
                    violations.Add($"{cardType.Name}: has always-present {capturedType.Name} tip but base description never mentions '{keyword}' (use a conditional lambda for upgrade-only keywords)");
            }
        }

        Assert.Empty(violations);
    }

    // Guards against lambda tips that return null for unupgraded cards.
    // Null tips crash the game with NullReferenceException in IHoverTip.RemoveDupes when hovering.
    // Upgrade-only tips must be added in OnUpgrade(), not via null-returning lambdas.
    //
    // Note: tips using HoverTipFactory.FromCard<T> may throw in tests (no game registry loaded).
    // Exceptions are skipped — they don't indicate a null-return bug.
    [Fact]
    public void AllCards_BaseCard_NoTipsReturnNull()
    {
        var violations = new List<string>();

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var tips = GetHoverTips(card);

            foreach (var tip in tips)
            {
                IHoverTip? result;
                try { result = tip.Tip(card); }
                catch { continue; } // exceptions are expected when game registry isn't loaded
                if (result == null)
                    violations.Add($"{cardType.Name}: tip returned null — add upgrade-only tips in OnUpgrade() instead of a null-returning lambda");
            }
        }

        Assert.Empty(violations);
    }

    // Spot-check: Fortitude's VulnerablePower tip must be upgrade-only (added in OnUpgrade),
    // because the base description only mentions Weak — not Vulnerable.
    [Fact]
    public void Fortitude_BaseCard_HasNoAlwaysPresentVulnerablePowerTip()
    {
        var tips = GetHoverTips(new Fortitude());
        Assert.False(HasTipForType(tips, typeof(VulnerablePower)),
            "Fortitude base card must not carry an always-present VulnerablePower tip — " +
            "base description only mentions Weak. Add it in OnUpgrade() instead.");
        Assert.True(HasTipForType(tips, typeof(WeakPower)),     "Fortitude missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(StrengthPower)), "Fortitude missing StrengthPower tip");
    }

    // Spot-checks for key EE cards — these fail with the card name in the message for fast diagnosis.
    [Fact]
    public void Lament_HasWeakPowerTip() =>
        Assert.True(HasTipForType(GetHoverTips(new Lament()), typeof(WeakPower)), "Lament missing WeakPower tip");

    [Fact]
    public void Discord_HasVulnerablePowerTip() =>
        Assert.True(HasTipForType(GetHoverTips(new Discord()), typeof(VulnerablePower)), "Discord missing VulnerablePower tip");

    [Fact]
    public void Canticle_HasStrengthPowerTip() =>
        Assert.True(HasTipForType(GetHoverTips(new Canticle()), typeof(StrengthPower)), "Canticle missing StrengthPower tip");

    [Fact]
    public void Inversion_HasAllFourStatusPowerTips()
    {
        var tips = GetHoverTips(new Inversion());
        Assert.True(HasTipForType(tips, typeof(WeakPower)),         "Inversion missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)),   "Inversion missing VulnerablePower tip");
        Assert.True(HasTipForType(tips, typeof(UnweakPower)),       "Inversion missing UnweakPower tip");
        Assert.True(HasTipForType(tips, typeof(UnvulnerablePower)), "Inversion missing UnvulnerablePower tip");
    }

    [Fact]
    public void Catharsis_HasWeakAndVulnerablePowerTips()
    {
        var tips = GetHoverTips(new Catharsis());
        Assert.True(HasTipForType(tips, typeof(WeakPower)),       "Catharsis missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)), "Catharsis missing VulnerablePower tip");
    }

    [Fact]
    public void TrueStrength_HasFourAlwaysPresentTipsPlusUpgradeOnlyStrength()
    {
        var tips = GetHoverTips(new TrueStrength());
        Assert.True(HasTipForType(tips, typeof(WeakPower)),         "TrueStrength missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)),   "TrueStrength missing VulnerablePower tip");
        Assert.True(HasTipForType(tips, typeof(UnweakPower)),       "TrueStrength missing UnweakPower tip");
        Assert.True(HasTipForType(tips, typeof(UnvulnerablePower)), "TrueStrength missing UnvulnerablePower tip");
        // Strength conversion is upgrade-only; StrengthPower tip must be a conditional lambda.
        Assert.False(HasTipForType(tips, typeof(StrengthPower)),
            "TrueStrength base card must not carry an always-present StrengthPower tip — " +
            "base description doesn't mention Strength. Use a conditional lambda.");
    }
}
