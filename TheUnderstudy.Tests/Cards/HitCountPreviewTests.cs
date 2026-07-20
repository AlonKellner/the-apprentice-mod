using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Contract for the live "(Hits N times)" preview on the "deal X damage for each Y" cards. Each such card
// must (1) back the hit count with a CalculatedVar named "CalculatedHits" over CalculationBase 0 /
// CalculationExtra 1 (so the previewed value equals the raw count), and (2) render it in cards.json via
// {CalculatedHits:diff()} + the plural formatter, gated to combat by {InCombat:(preview)|}. Mirrors base-game
// FlakCannon/Flechettes. The live value itself needs a real Owner/combat, so it's verified in-game — these
// bare tests only pin the var wiring and the loc reference.
public class HitCountPreviewTests
{
    public static IEnumerable<object[]> HitCountCards() => new List<object[]>
    {
        new object[] { typeof(CleanSlate) },
        new object[] { typeof(LoosenUp) },
    };

    [Theory]
    [MemberData(nameof(HitCountCards))]
    public void Card_BacksHitCountWithCalculatedVar(Type cardType)
    {
        var card = (UnderstudyCard)Activator.CreateInstance(cardType)!;

        Assert.True(card.DynamicVars.ContainsKey("CalculatedHits"), $"{cardType.Name}: no 'CalculatedHits' var");
        Assert.IsType<CalculatedVar>(card.DynamicVars["CalculatedHits"]);

        // base 0 + extra 1 * count => the previewed value is exactly the raw hit count.
        Assert.True(card.DynamicVars.ContainsKey("CalculationBase"), $"{cardType.Name}: no 'CalculationBase' var");
        Assert.True(card.DynamicVars.ContainsKey("CalculationExtra"), $"{cardType.Name}: no 'CalculationExtra' var");
        Assert.Equal(0, (int)card.DynamicVars["CalculationBase"].BaseValue);
        Assert.Equal(1, (int)card.DynamicVars["CalculationExtra"].BaseValue);
    }

    [Theory]
    [MemberData(nameof(HitCountCards))]
    public void Card_RendersHitCountPreview(Type cardType)
    {
        string key = "THEUNDERSTUDY-" + ToScreamingSnakeCase(cardType.Name);
        var description = LoadDescriptions()[key];

        Assert.Contains("{CalculatedHits:diff()}", description);
        Assert.Contains("{CalculatedHits:plural:time|times}", description);
        // Gated to combat-only via the base-game pattern: the preview sits in the InCombat TRUE branch
        // ({InCombat:(preview)|}), so it shows during combat and not in the Compendium.
        Assert.Contains("{InCombat:\n(Hits {CalculatedHits:diff()}", description);
    }

    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static Dictionary<string, string> LoadDescriptions()
    {
        var path = Path.Combine(RepoRoot, "TheUnderstudy", "localization", "eng", "cards.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var result = new Dictionary<string, string>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            const string suffix = ".description";
            if (prop.Name.EndsWith(suffix))
                result[prop.Name[..^suffix.Length]] = prop.Value.GetString() ?? "";
        }
        return result;
    }

    private static string ToScreamingSnakeCase(string pascalCase)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < pascalCase.Length; i++)
        {
            char c = pascalCase[i];
            if (i > 0 && char.IsUpper(c))
            {
                bool afterLower = char.IsLower(pascalCase[i - 1]);
                bool endOfAcronymRun = char.IsUpper(pascalCase[i - 1]) && i + 1 < pascalCase.Length && char.IsLower(pascalCase[i + 1]);
                if (afterLower || endOfAcronymRun)
                    sb.Append('_');
            }
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }
}
