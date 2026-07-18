using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Contract for the Pulled-Punch-aware colored preview on self-debuff cards: each card that applies an
// invertible debuff to yourself must (1) back that number with a SelfDebuffVar of the native amount, and
// (2) render it in cards.json via {<Debuff>:inverseDiff()} so it colors green when Pulled Punch lowers it.
public class SelfDebuffPreviewTests
{
    // Card type -> (var/debuff name, native amount). One self-debuff each. Pathos (also hits enemies,
    // deliberately excluded) is handled separately. Post-redesign, Tension is no longer applied by any
    // card; new self-debuff cards are added here as they are created.
    public static IEnumerable<object[]> SelfDebuffCards() => new List<object[]>
    {
        new object[] { typeof(FreezeUp), "Weak", 1 },
        new object[] { typeof(DesperateStrike), "Weak", 2 },
        new object[] { typeof(BreakingVoice), "Weak", 2 },
        new object[] { typeof(HeartAche), "Vulnerable", 1 },
        new object[] { typeof(Joke), "Vulnerable", 1 },
        new object[] { typeof(TheWall), "Vulnerable", 1 },
    };

    [Theory]
    [MemberData(nameof(SelfDebuffCards))]
    public void Card_BacksSelfDebuffWithVar(Type cardType, string debuff, int amount)
    {
        var card = (UnderstudyCard)Activator.CreateInstance(cardType)!;
        Assert.True(card.DynamicVars.ContainsKey(debuff),
            $"{cardType.Name}: no DynamicVar named '{debuff}'");
        var var = card.DynamicVars[debuff];
        Assert.IsType<SelfDebuffVar>(var);
        Assert.Equal(amount, (int)var.BaseValue);
    }

    [Theory]
    [MemberData(nameof(SelfDebuffCards))]
    public void Card_RendersSelfDebuffWithInverseDiff(Type cardType, string debuff, int amount)
    {
        string key = "THEUNDERSTUDY-" + ToScreamingSnakeCase(cardType.Name);
        var description = LoadDescriptions()[key];
        Assert.Contains($"{{{debuff}:inverseDiff()}}", description);
        // The hard-coded literal must be gone — the colored var replaces it.
        Assert.DoesNotContain($"Apply {amount} [gold]{debuff}[/gold] to yourself", description);
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
