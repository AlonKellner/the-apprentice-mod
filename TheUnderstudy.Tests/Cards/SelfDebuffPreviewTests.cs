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
    // Card type -> (var/debuff name, native amount). One self-debuff each. DressRehearsal (5 debuffs from
    // one var) and Ensemble (also hits enemies, deliberately excluded) are covered/handled separately.
    public static IEnumerable<object[]> SelfDebuffCards() => new List<object[]>
    {
        new object[] { typeof(FreezeUp), "Weak", 1 },
        new object[] { typeof(StageWhisper), "Weak", 2 },
        new object[] { typeof(Understatement), "Weak", 2 },
        new object[] { typeof(WindUp), "Weak", 2 },
        new object[] { typeof(Downstage), "Vulnerable", 1 },
        new object[] { typeof(Overcommit), "Vulnerable", 1 },
        new object[] { typeof(TrueColors), "Vulnerable", 1 },
        new object[] { typeof(WideOpen), "Vulnerable", 1 },
        new object[] { typeof(Butterflies), "Shaken", 2 },
        new object[] { typeof(MissedCue), "Shaken", 2 },
        new object[] { typeof(OpeningNumber), "Shaken", 1 },
        new object[] { typeof(TakeCenterStage), "Shaken", 2 },
        new object[] { typeof(Flourish), "Limited", 1 },
        new object[] { typeof(FastForward), "Limited", 2 },
        new object[] { typeof(OffScript), "Limited", 1 },
        new object[] { typeof(Overexert), "Limited", 2 },
        new object[] { typeof(Matinee), "Jaded", 1 },
        new object[] { typeof(QuickNap), "Jaded", 1 },
        new object[] { typeof(Rerun), "Jaded", 2 },
        new object[] { typeof(AllNighter), "Jaded", 2 },
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

    // Dress Rehearsal applies `amount` of 5 debuffs to self AND schedules an Invert of `amount` next
    // turn. Pulled Punch reduces the applied debuffs but not the Invert, so the two numbers are backed
    // by different vars: a SelfDebuffVar (colored inverseDiff) for the apply line, and the untouched
    // DressRehearsalPower (normal diff) for the Invert line.
    [Fact]
    public void DressRehearsal_BacksSelfDebuffWithVar()
    {
        var card = new DressRehearsal();
        Assert.True(card.DynamicVars.ContainsKey("SelfDebuff"));
        Assert.IsType<SelfDebuffVar>(card.DynamicVars["SelfDebuff"]);
        Assert.Equal(2, (int)card.DynamicVars["SelfDebuff"].BaseValue);
    }

    [Fact]
    public void DressRehearsal_Loc_ApplyLineInverseDiff_InvertLineNormalDiff()
    {
        var desc = LoadDescriptions()["THEUNDERSTUDY-DRESS_REHEARSAL"];
        Assert.Contains("{SelfDebuff:inverseDiff()}", desc);       // self-debuff apply line (colored)
        Assert.Contains("{DressRehearsalPower:diff()}", desc);      // Invert line, unchanged by Pulled Punch
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
