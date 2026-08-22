using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Every in-combat card-selection prompt must state HOW MANY cards to pick, via the {Amount} token that
// CardSelectorPrefs injects (Prompt.Add("Amount", count)) and the selection UI renders. Guards the
// requirement "the prompt specifies the amount" — e.g. Run Through's "Choose 2 cards to make Tuned".
// Scans localization directly (no ModelDb), like SwapPhrasingTests / CardTooltipKeywordSyncTests.
public class SelectionPromptTests
{
    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static IEnumerable<object[]> SelectionPrompts()
    {
        var path = Path.Combine(RepoRoot, "TheUnderstudy", "localization", "eng", "cards.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var prop in doc.RootElement.EnumerateObject())
            if (prop.Name.EndsWith(".selectionPrompt"))
                yield return new object[] { prop.Name, prop.Value.GetString() ?? "" };
    }

    [Theory]
    [MemberData(nameof(SelectionPrompts))]
    public void SelectionPrompt_StatesTheAmount(string key, string prompt)
    {
        Assert.True(prompt.Contains("{Amount}"),
            $"{key}: selection prompts must state the count via the {{Amount}} token so the player sees how " +
            $"many cards to pick (e.g. \"Choose {{Amount}} cards to make [gold]Tuned[/gold].\"). Got: \"{prompt}\"");
    }
}
