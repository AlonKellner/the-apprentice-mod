using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Card selections must require EXACTLY N cards, never "up to N". In CardSelectorPrefs, "up to N" is a
// literal 0 minimum — new CardSelectorPrefs(prompt, 0, N) — while exact-count is the single-count ctor
// (prompt, N) or an equal min/max (prompt, N, N). This source scan fails on any 0-minimum selection, the
// standing guard that every applier forces the full amount. Same file-scan approach as PlayAllPlannedCardTests.
public class ExactSelectionCountTests
{
    private static string CardsDir => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "TheUnderstudyCode", "Cards"));

    [Fact]
    public void NoCardSelection_UsesAZeroMinimum()
    {
        Assert.True(Directory.Exists(CardsDir), $"Cards dir not found: {CardsDir}");

        var offenders = new List<string>();
        foreach (var file in Directory.GetFiles(CardsDir, "*.cs", SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
                // A CardSelectorPrefs line whose min argument is a literal 0 (", 0,") is "up to N".
                if (lines[i].Contains("CardSelectorPrefs") && Regex.IsMatch(lines[i], @",\s*0\s*,"))
                    offenders.Add($"{Path.GetFileName(file)}:{i + 1}");
        }

        Assert.True(offenders.Count == 0,
            "Card selections must require exactly N (no 'up to N' with a 0 minimum) — use new " +
            "CardSelectorPrefs(prompt, N) for hand or ExactPileSelection for piles. Offenders: " +
            string.Join(", ", offenders));
    }
}
