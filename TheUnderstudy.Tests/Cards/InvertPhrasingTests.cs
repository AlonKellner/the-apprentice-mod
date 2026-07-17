using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Enforces the unified Invert phrasing: after the "of each" refactor, every card that mentions
// [gold]Invert[/gold] must read simply "Invert N." with no qualifier clause, and the Invert
// keyword tooltip must explain the "each" meaning. Scans localization text directly (no ModelDb),
// the same file-scanning approach CardTooltipKeywordSyncTests uses.
public class InvertPhrasingTests
{
    // Qualifier clauses that the old per-variant phrasings used. None may appear alongside an
    // [gold]Invert[/gold] mention any more — the meaning now lives entirely in the keyword tooltip.
    private static readonly string[] BannedQualifiers =
    {
        "of each",
        "of the last",
        "last modified",
        "of the next",
        "of a random",
        "invertible buffs and debuffs",
        "invertible debuff you",
        "random [gold]invertible[/gold]",
    };

    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static Dictionary<string, string> LoadJsonStrings(string relativePath)
    {
        var path = Path.Combine(RepoRoot, relativePath);
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var result = new Dictionary<string, string>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            result[prop.Name] = prop.Value.GetString() ?? "";
        return result;
    }

    public static IEnumerable<object[]> CardDescriptions()
    {
        foreach (var (key, value) in LoadJsonStrings(Path.Combine("TheUnderstudy", "localization", "eng", "cards.json")))
        {
            if (!key.EndsWith(".description")) continue;
            if (!value.Contains("[gold]Invert[/gold]")) continue;
            yield return new object[] { key, value };
        }
    }

    [Theory]
    [MemberData(nameof(CardDescriptions))]
    public void InvertDescriptions_UseUnifiedPhrasing(string key, string description)
    {
        var offenders = BannedQualifiers.Where(q => description.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.True(offenders.Count == 0,
            $"{key}: Invert description still contains banned qualifier(s): {string.Join(", ", offenders)}. " +
            "Invert must read simply 'Invert N.' — the meaning lives in the keyword tooltip.");
    }

    [Fact]
    public void InvertKeyword_TooltipMentionsEach()
    {
        var keywords = LoadJsonStrings(Path.Combine("TheUnderstudy", "localization", "eng", "card_keywords.json"));
        Assert.True(keywords.TryGetValue("THEUNDERSTUDY-INVERT.description", out var desc),
            "Missing THEUNDERSTUDY-INVERT.description in card_keywords.json");
        Assert.Contains("each", desc, StringComparison.OrdinalIgnoreCase);
    }
}
