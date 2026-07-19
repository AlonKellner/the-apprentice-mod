using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Enforces the reworked Swap phrasing: Swap has no numeric magnitude any more — the card number is a
// repeat count rendered as "Swap" / "Swap twice" / "Swap N times". So no card that mentions
// [gold]Swap[/gold] may still print a raw Swap amount (the old {Swap:diff()} var or an "of each"
// qualifier); the meaning lives in the keyword tooltip. Scans localization text directly (no ModelDb),
// the same file-scanning approach InvertPhrasingTests / CardTooltipKeywordSyncTests use.
public class SwapPhrasingTests
{
    // Fragments that only existed in the old magnitude-based phrasing. None may appear alongside a
    // [gold]Swap[/gold] mention any more.
    private static readonly string[] BannedFragments =
    {
        "{Swap",                 // the old {Swap:diff()} / {Swap} numeric var
        "{StagePresencePower",   // the old passive's numeric var
        "of each swappable",     // old "give this many of each swappable ..."
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

    public static IEnumerable<object[]> SwapCardDescriptions()
    {
        foreach (var (key, value) in LoadJsonStrings(Path.Combine("TheUnderstudy", "localization", "eng", "cards.json")))
        {
            if (!key.EndsWith(".description")) continue;
            if (!value.Contains("[gold]Swap[/gold]")) continue;
            yield return new object[] { key, value };
        }
    }

    [Theory]
    [MemberData(nameof(SwapCardDescriptions))]
    public void SwapDescriptions_CarryNoNumericMagnitude(string key, string description)
    {
        var offenders = BannedFragments.Where(f => description.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.True(offenders.Count == 0,
            $"{key}: Swap description still contains banned fragment(s): {string.Join(", ", offenders)}. " +
            "Swap has no value — the card number is a repeat count ('Swap' / 'Swap twice' / 'Swap N times'), " +
            "and the meaning lives in the keyword tooltip.");
    }

    [Fact]
    public void SwapKeyword_TooltipDescribesSingleTarget()
    {
        var keywords = LoadJsonStrings(Path.Combine("TheUnderstudy", "localization", "eng", "card_keywords.json"));
        Assert.True(keywords.TryGetValue("THEUNDERSTUDY-SWAP.description", out var desc),
            "Missing THEUNDERSTUDY-SWAP.description in card_keywords.json");
        // The meaning now lives here: the "most recently changed" single-target give/take.
        Assert.Contains("most recently", desc, StringComparison.OrdinalIgnoreCase);
    }
}
