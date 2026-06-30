using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
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

                json.TryGetValue($"{locPrefix}.description", out var baseDesc);

                // Strip {IfUpgraded:show:upgrade|base} to get the base-card rendering,
                // since the upgrade path may mention tokenName+ even when the base card does not.
                var baseRendered = Regex.Replace(baseDesc ?? "", @"\{IfUpgraded:show:[^|]*\|([^}]*)\}", "$1");

                if (baseRendered.Contains(tokenName + "+"))
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
    // appear in the BASE description. typeof(T) tips are always-present (shown on unupgraded card),
    // so their keywords must be explained in the base description — not just the upgrade description.
    //
    // Upgrade-only tips must use conditional lambdas in the constructor (not OnUpgrade()), because
    // OnUpgrade() is not called when loading upgraded cards from save. This test doesn't flag lambdas.
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
    // Upgrade-only tips must use conditional lambdas whose non-upgraded branch returns a non-null tip
    // (e.g., duplicate an existing always-present tip — RemoveDupes deduplicates by ID).
    //
    // Note: tips using HoverTipFactory.FromCard<T> or typeof(T) may throw in tests (no game
    // registry loaded). Exceptions are skipped — they don't indicate a null-return bug.
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

    private static FieldInfo GetCurrentUpgradeLevelField()
    {
        var t = typeof(ConstructedCardModel).BaseType ?? typeof(ConstructedCardModel);
        while (t != null)
        {
            var f = t.GetField("_currentUpgradeLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null) return f;
            t = t.BaseType;
        }
        // Also try ConstructedCardModel itself
        var f2 = typeof(ConstructedCardModel).GetField("_currentUpgradeLevel", BindingFlags.NonPublic | BindingFlags.Instance);
        if (f2 != null) return f2;
        throw new InvalidOperationException("_currentUpgradeLevel field not found on CardModel hierarchy");
    }

    // Simulate how the game loads an upgraded card: set _currentUpgradeLevel directly.
    // The game does NOT call OnUpgrade() when loading from save — only sets the level field.
    // Tips added in OnUpgrade() are therefore LOST after reload, making OnUpgrade() wrong
    // for upgrade-conditional tooltips.
    private static T AsUpgraded<T>(T card) where T : ConstructedCardModel
    {
        GetCurrentUpgradeLevelField().SetValue(card, 1);
        return card;
    }

    // Spot-check: Fortitude's VulnerablePower tip must be upgrade-only, because the base
    // description only mentions Weak — not Vulnerable.
    // After the lambda fix: the tip is in _hoverTips as a conditional lambda (not typeof(T)),
    // so HasTipForType (which uses GetCapturedType) correctly returns false for the base card.
    [Fact]
    public void Fortitude_BaseCard_HasNoAlwaysPresentVulnerablePowerTip()
    {
        var tips = GetHoverTips(new Fortitude());
        Assert.False(HasTipForType(tips, typeof(VulnerablePower)),
            "Fortitude base card must not carry an always-present VulnerablePower typeof(T) tip — " +
            "base description only mentions Weak. Use a conditional lambda for upgrade-only keywords.");
        Assert.True(HasTipForType(tips, typeof(WeakPower)),   "Fortitude missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VigorPower)), "Fortitude missing VigorPower tip");
    }

    // Fortitude has no upgrade-only keywords — both base and upgrade mention Weak and Vigor.
    // Upgrade only increases the Vigor gain (3 → 5), handled by the power's Amount.
    [Fact]
    public void Fortitude_HasNoConditionalLambdaTip()
    {
        var tips = GetHoverTips(new Fortitude());
        Assert.True(tips.All(UsesImplicitTypeConversion),
            "Fortitude has no upgrade-only keywords — all tips must be always-present typeof(T) tips, not conditional lambdas.");
    }

    // Comprehensive: for every card where the upgrade description introduces a new power keyword
    // not present in the base description, there must be a conditional lambda tip (non-typeof(T))
    // in the constructor. typeof(T) tips are always-present; OnUpgrade() tips are NOT present when
    // loading upgraded cards from save (the game only sets _currentUpgradeLevel, not calling OnUpgrade()).
    [Fact]
    public void AllCards_UpgradeOnlyPowerKeywords_HaveConditionalLambdaTip()
    {
        var json = LoadCardsJson();
        var violations = new List<string>();

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var locPrefix = ToLocKeyPrefix(cardType);

            json.TryGetValue($"{locPrefix}.description",  out var baseDesc);
            json.TryGetValue($"{locPrefix}+.description", out var upgDesc);

            if (upgDesc == null) continue;

            var tips = GetHoverTips(card);
            bool hasAnyConditionalLambda = tips.Any(tip => !UsesImplicitTypeConversion(tip));

            foreach (var (keyword, _) in PowerKeywordToType)
            {
                var tag = $"[gold]{keyword}[/gold]";
                if (baseDesc?.Contains(tag) == true) continue; // base has it → typeof(T) tip covers it
                if (!upgDesc.Contains(tag)) continue;          // neither has it → no tip needed

                if (!hasAnyConditionalLambda)
                    violations.Add(
                        $"{cardType.Name}: '{keyword}' appears only in +.description but no " +
                        "conditional lambda tip found. OnUpgrade() is not called when loading " +
                        "upgraded cards from save. Add a lambda in the constructor that checks " +
                        "card.IsUpgraded at display time.");
            }
        }

        Assert.Empty(violations);
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
    public void Transference_BaseCard_HasWeakAndVulnerablePowerTips()
    {
        var tips = GetHoverTips(new Transference());
        Assert.True(HasTipForType(tips, typeof(WeakPower)),       "Transference missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)), "Transference missing VulnerablePower tip");
        Assert.True(tips.All(UsesImplicitTypeConversion),
            "Transference has no upgrade-only keywords — all tips must be typeof(T), not conditional lambdas.");
    }

    [Fact]
    public void Recrimination_BaseCard_HasWeakAndVulnerablePowerTips()
    {
        var tips = GetHoverTips(new Recrimination());
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)), "Recrimination missing VulnerablePower tip");
        Assert.True(HasTipForType(tips, typeof(WeakPower)),       "Recrimination missing WeakPower tip");
        Assert.True(tips.All(UsesImplicitTypeConversion),
            "Recrimination has no upgrade-only keywords — all tips must be typeof(T), not conditional lambdas.");
    }

    [Fact]
    public void Fermata_HasExactlyFourAlwaysPresentTips_NoLambda_NoStrength()
    {
        var tips = GetHoverTips(new Fermata());
        Assert.True(HasTipForType(tips, typeof(WeakPower)),         "Fermata missing WeakPower tip");
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)),   "Fermata missing VulnerablePower tip");
        Assert.True(HasTipForType(tips, typeof(UnweakPower)),       "Fermata missing UnweakPower tip");
        Assert.True(HasTipForType(tips, typeof(UnvulnerablePower)), "Fermata missing UnvulnerablePower tip");
        Assert.False(HasTipForType(tips, typeof(StrengthPower)),
            "Fermata must not have a StrengthPower tip.");
        Assert.True(tips.All(UsesImplicitTypeConversion),
            "Fermata has no upgrade-only keywords — all tips must be typeof(T), not conditional lambdas.");
    }

    // Detects the power-keyword lambda anti-pattern: a TooltipSource whose closure captures
    // another TooltipSource (i.e. `card => card.IsUpgraded ? powerTip : otherPowerTip`).
    // This is distinct from legitimate token-display lambdas, which capture nothing.
    private static bool HasPowerKeywordLambdaAntiPattern(TooltipSource tip)
    {
        if (UsesImplicitTypeConversion(tip)) return false;
        var makeTipField = typeof(TooltipSource)
            .GetField("_makeTip", BindingFlags.NonPublic | BindingFlags.Instance);
        var makeTip = makeTipField?.GetValue(tip) as Delegate;
        if (makeTip?.Target == null) return false;
        return makeTip.Target.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Any(f => f.FieldType == typeof(TooltipSource));
    }

    // Bidirectional guard: if +.description introduces no new power keywords vs .description,
    // then the card must not use conditional power-keyword lambda tips (closures that capture
    // TooltipSource to switch between power tips based on IsUpgraded). Legitimate token-display
    // lambdas (which do not capture TooltipSource) are NOT flagged.
    [Fact]
    public void AllCards_NoConditionalLambdaTips_UnlessUpgradeDescriptionHasNewKeywords()
    {
        var json = LoadCardsJson();
        var violations = new List<string>();

        foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
        {
            var locPrefix = ToLocKeyPrefix(cardType);
            json.TryGetValue($"{locPrefix}.description",  out var baseDesc);
            json.TryGetValue($"{locPrefix}+.description", out var upgDesc);

            if (upgDesc == null) continue;

            bool upgradeHasNewKeyword = PowerKeywordToType.Keys.Any(keyword =>
            {
                var tag = $"[gold]{keyword}[/gold]";
                return upgDesc.Contains(tag) && baseDesc?.Contains(tag) != true;
            });

            if (upgradeHasNewKeyword) continue;

            var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
            var tips = GetHoverTips(card);

            if (tips.Any(HasPowerKeywordLambdaAntiPattern))
                violations.Add(
                    $"{cardType.Name}: +.description introduces no new power keywords " +
                    "but card has conditional power-keyword lambda tips — use typeof(T) tips.");
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void Undercurrent_HasVulnerablePowerTip_NotWeakPowerTip()
    {
        var tips = GetHoverTips(new Undercurrent());
        Assert.True(HasTipForType(tips, typeof(VulnerablePower)), "Undercurrent must have a VulnerablePower tip");
        Assert.False(HasTipForType(tips, typeof(WeakPower)), "Undercurrent must not have a WeakPower tip after retargeting");
    }

    // Detects token card type from a TooltipSource via two strategies:
    // 1. typeof(T) implicit conversion: captured System.Type field in the closure
    // 2. HoverTipFactory.FromCard<T>() lambda: scan IL of the closure method for a call
    //    to a generic HoverTipFactory.FromCard<T> and extract the generic argument
    private static Type? GetTokenTypeFromTip(TooltipSource tip)
    {
        var capturedType = GetCapturedType(tip);
        if (capturedType == typeof(Dream) || capturedType == typeof(Ambition) || capturedType == typeof(Potential))
            return capturedType;

        var makeTipField = typeof(TooltipSource).GetField("_makeTip", BindingFlags.NonPublic | BindingFlags.Instance);
        var makeTip = makeTipField?.GetValue(tip) as Delegate;
        if (makeTip == null) return null;
        return ScanLambdaForFromCardType(makeTip.Method);
    }

    private static readonly HashSet<Type> TokenCardTypes = new() { typeof(Dream), typeof(Ambition), typeof(Potential) };

    private static Type? ScanLambdaForFromCardType(MethodBase method)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il == null) return null;
        int i = 0;
        while (i < il.Length)
        {
            byte b = il[i];
            if ((b == 0x28 || b == 0x6F) && i + 4 < il.Length)
            {
                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    var resolved = method.Module.ResolveMethod(token);
                    if (resolved is MethodInfo mi && mi.IsGenericMethod && mi.Name == "FromCard" &&
                        mi.DeclaringType?.Name == "HoverTipFactory")
                    {
                        var arg = mi.GetGenericArguments().FirstOrDefault();
                        if (arg != null && TokenCardTypes.Contains(arg)) return arg;
                    }
                }
                catch { }
                i += 5;
            }
            else if ((b >= 0x72 && b <= 0x80) || b == 0x8C || b == 0x8D || b == 0xD0 || b == 0x29 ||
                     b == 0x20 || b == 0x21 || b == 0x22 || b == 0x23)
            {
                i += 5; // instructions with 4-byte operands
            }
            else if (b == 0x0E || b == 0x0F || b == 0x10 || b == 0x11 || b == 0x12 ||
                     b == 0x13 || b == 0x1F || (b >= 0x2B && b <= 0x37))
            {
                i += 2; // instructions with 1-byte operands
            }
            else if (b == 0xFE)
            {
                i += 2; // two-byte instruction prefix
            }
            else
            {
                i += 1;
            }
        }
        return null;
    }

    private static CardKeyword? GetCapturedCardKeyword(TooltipSource tip)
    {
        var makeTipField = typeof(TooltipSource).GetField("_makeTip", BindingFlags.NonPublic | BindingFlags.Instance);
        var makeTip = makeTipField?.GetValue(tip) as Delegate;
        if (makeTip?.Target == null) return null;
        var kField = makeTip.Target.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(CardKeyword));
        if (kField == null) return null;
        return (CardKeyword)kField.GetValue(makeTip.Target)!;
    }

    // Custom enum values (ApprenticeKeywords.*) are 0 outside the game runtime because the
    // Harmony patch that assigns them only runs during ModelDb.Init(). We temporarily inject
    // distinct sentinel values so GetCapturedCardKeyword can tell the keywords apart.
    [Fact]
    public void AllCards_TokenPreviewTips_HaveMatchingKeywordTips()
    {
        var savedDreamy   = ApprenticeKeywords.Dreamy;
        var savedAmbitous = ApprenticeKeywords.Ambitous;
        var savedExpend   = ApprenticeKeywords.Expend;
        ApprenticeKeywords.Dreamy   = (CardKeyword)1001;
        ApprenticeKeywords.Ambitous = (CardKeyword)1002;
        ApprenticeKeywords.Expend   = (CardKeyword)1003;

        try
        {
            var violations = new List<string>();

            foreach (var cardType in typeof(ApprenticeCard).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ApprenticeCard))))
            {
                var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
                var tips = GetHoverTips(card);

                var tokenTypes = tips
                    .Select(GetTokenTypeFromTip)
                    .Where(t => t != null).Select(t => t!).ToHashSet();

                var keywords = tips
                    .Select(GetCapturedCardKeyword)
                    .Where(k => k.HasValue).Select(k => k!.Value).ToHashSet();

                bool hasDreamy   = keywords.Contains((CardKeyword)1001);
                bool hasAmbitous = keywords.Contains((CardKeyword)1002);
                bool hasExpend   = keywords.Contains((CardKeyword)1003);

                if (tokenTypes.Contains(typeof(Dream)))
                {
                    if (!hasDreamy)
                        violations.Add($"{cardType.Name}: has Dream tip but missing Dreamy keyword tip");
                    if (!hasExpend)
                        violations.Add($"{cardType.Name}: has Dream tip but missing Expend keyword tip");
                }
                if (tokenTypes.Contains(typeof(Ambition)))
                {
                    if (!hasAmbitous)
                        violations.Add($"{cardType.Name}: has Ambition tip but missing Ambitous keyword tip");
                    if (!hasExpend)
                        violations.Add($"{cardType.Name}: has Ambition tip but missing Expend keyword tip");
                }
                if (tokenTypes.Contains(typeof(Potential)))
                {
                    if (!hasAmbitous)
                        violations.Add($"{cardType.Name}: has Potential tip but missing Ambitous keyword tip");
                    if (!hasDreamy)
                        violations.Add($"{cardType.Name}: has Potential tip but missing Dreamy keyword tip");
                    if (!hasExpend)
                        violations.Add($"{cardType.Name}: has Potential tip but missing Expend keyword tip");
                }
            }

            Assert.Empty(violations);
        }
        finally
        {
            ApprenticeKeywords.Dreamy   = savedDreamy;
            ApprenticeKeywords.Ambitous = savedAmbitous;
            ApprenticeKeywords.Expend   = savedExpend;
        }
    }

    [Fact]
    public void Scapegoat_DescriptionDoesNotMentionStrength()
    {
        var json = LoadCardsJson();
        Assert.True(json.TryGetValue("THEAPPRENTICE-SCAPEGOAT.description", out var desc),
            "THEAPPRENTICE-SCAPEGOAT.description key not found in cards.json");
        Assert.DoesNotContain("Strength", desc, StringComparison.OrdinalIgnoreCase);
    }

    // Fails before fix: AchingWish, Wishful, Passion, and Blueprint tie their Dream/Ambition
    // preview tip to card.IsUpgraded via a conditional lambda (HoverTipFactory.FromCard<T>(upgrade:
    // card.IsUpgraded)), which renders "Dream+"/"Ambition+" once the host card is upgraded. But
    // none of these cards' OnPlay ever upgrades the Dream/Ambition token it creates — upgrading
    // the host card only changes a count (3->4 Dreams) or another stat (damage/block), never the
    // token's own upgrade state. The tip should therefore always preview the unupgraded token.
    // Passes after fix: tip uses typeof(T), whose implicit conversion (see BaseLib's
    // TooltipSource.implicit operator(Type)) always passes upgrade: false.
    [Theory]
    [InlineData(typeof(AchingWish), typeof(Dream))]
    [InlineData(typeof(Wishful), typeof(Dream))]
    [InlineData(typeof(Passion), typeof(Dream))]
    [InlineData(typeof(Passion), typeof(Ambition))]
    [InlineData(typeof(Blueprint), typeof(Dream))]
    [InlineData(typeof(Blueprint), typeof(Ambition))]
    public void Card_WhoseUpgradeNeverProducesUpgradedToken_UsesImplicitTypeConversionForTokenTip(
        Type cardType, Type tokenType)
    {
        var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
        var tips = GetHoverTips(card);
        var tip = tips.FirstOrDefault(t => GetTokenTypeFromTip(t) == tokenType);

        Assert.NotNull(tip);
        Assert.True(UsesImplicitTypeConversion(tip!),
            $"{cardType.Name}'s {tokenType.Name} tip must use typeof({tokenType.Name}) (always " +
            $"unupgraded) — {cardType.Name}'s own upgrade never causes the created {tokenType.Name} " +
            "token to become upgraded.");
    }

    // Fails before fix: Tuning and Suspension both add a typeof(T) hover tip pointing at their
    // own granted power. typeof(T) tips render via PowerModel.GetDumbHoverTip, which reads the
    // ModelDb *template* instance's Amount (always 0) — never the live stack count — so hovering
    // the card shows "gain 0 additional Tension" / "gain 0 Tension and 0 Vigor". It's also
    // redundant: the card's own description already states the effect (see e.g. Conviction,
    // Lullaby, Conductor, Wunderkind, none of which self-tip their granted power).
    // Passes after fix: the self-referential typeof(T) tip is removed.
    [Theory]
    [InlineData(typeof(Tuning), typeof(TuningPower))]
    [InlineData(typeof(Suspension), typeof(SuspensionPower))]
    [InlineData(typeof(Fortissimo), typeof(FortissimoPower))]
    public void Card_DoesNotTipItsOwnGrantedPower(Type cardType, Type ownPowerType)
    {
        var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
        var tips = GetHoverTips(card);
        Assert.False(HasTipForType(tips, ownPowerType),
            $"{cardType.Name} must not have a self-referential {ownPowerType.Name} tip — " +
            "typeof(T) tips read the ModelDb template's Amount (always 0), and the card's own " +
            "description already states the effect.");
    }

    // Both Tuning and Suspension still need to explain Tension itself (a concept they reference
    // but don't grant directly via their own power), so the TensionPower tip must remain.
    [Theory]
    [InlineData(typeof(Tuning))]
    [InlineData(typeof(Suspension))]
    [InlineData(typeof(Fortissimo))]
    public void Card_StillHasTensionPowerTip(Type cardType)
    {
        var card = (ConstructedCardModel)Activator.CreateInstance(cardType)!;
        var tips = GetHoverTips(card);
        Assert.True(HasTipForType(tips, typeof(TensionPower)),
            $"{cardType.Name} must still have a TensionPower tip explaining what Tension does.");
    }
}
