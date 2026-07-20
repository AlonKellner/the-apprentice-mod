using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Contract for Fate Knocking's Body-Slam-style finisher preview. The finisher deals
// ModifyDamage(priorSum + 3 * modified-strike); a display-only CalculatedDamageVar whose raw value is
// (priorSum + 3 * modified-strike) reproduces that exactly. These bare tests pin the pure arithmetic and the
// var/loc wiring; the modifier-exact runtime value needs a live combat/target and is verified in-game.
public class FateKnockingPreviewTests
{
    // The "account for the 3 upcoming hits" core: raw finisher base = priorSum + strikes * perStrikeDamage.
    [Theory]
    [InlineData(100, 3, 4, 112)]
    [InlineData(0, 3, 0, 0)]
    [InlineData(0, 3, 3, 9)]
    [InlineData(50, 3, 0, 50)]
    [InlineData(10, 3, 5, 25)]
    public void ComputeFinisherBase_SumsPriorPlusStrikes(int prior, int strikes, int perStrike, int expected)
    {
        Assert.Equal(expected, (int)FateKnocking.ComputeFinisherBase(prior, strikes, perStrike));
    }

    [Fact]
    public void Card_BacksFinisherPreviewWithCalculatedDamageVar()
    {
        var card = new FateKnocking();

        Assert.True(card.DynamicVars.ContainsKey("CalculatedDamage"), "no 'CalculatedDamage' var");
        Assert.IsType<CalculatedDamageVar>(card.DynamicVars["CalculatedDamage"]);

        // raw = CalculationBase(0) + ExtraDamage(1) * multiplier => the multiplier value verbatim.
        Assert.True(card.DynamicVars.ContainsKey("CalculationBase"), "no 'CalculationBase' var");
        Assert.True(card.DynamicVars.ContainsKey("ExtraDamage"), "no 'ExtraDamage' var");
        Assert.Equal(0, (int)card.DynamicVars["CalculationBase"].BaseValue);
        Assert.Equal(1, (int)card.DynamicVars["ExtraDamage"].BaseValue);
    }

    [Fact]
    public void Card_RendersFinisherPreview()
    {
        var description = LoadDescriptions()["THEUNDERSTUDY-FATE_KNOCKING"];
        Assert.Contains("{CalculatedDamage:diff()}", description);
        // Combat-only via the base-game pattern: preview in the InCombat TRUE branch ({InCombat:(preview)|}).
        Assert.Contains("{InCombat:\n(Deals {CalculatedDamage:diff()} damage)|}", description);
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
}
