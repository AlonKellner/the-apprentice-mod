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
        Assert.Equal(expected, (int)FateKnocking.ComputeFinisherBase(prior, strikes, perStrike, 0m));
    }

    // Vigor is baked into perStrikeDamage AND re-added by ModifyDamage on top of the finisher's total, but
    // the strikes consume it before the finisher's own attack command runs. Subtracting it from the raw base
    // is what makes the two cancel — the previewed number was otherwise Vigor too high on every play with
    // Vigor up (the mismatch the FateKnocking.OnPlay invariant caught: previewed 19 vs. a rolled 15).
    [Theory]
    // 3 strikes of (1 base + 4 Vigor) = 15 raw; ModifyDamage re-adds the 4, so the base must hand back 11.
    [InlineData(0, 3, 5, 4, 11)]
    [InlineData(10, 3, 5, 4, 21)]
    // Vigor can be negative (VigorAllowNegativePatch), and the cancellation is symmetric.
    [InlineData(0, 3, 2, -3, 9)]
    // No Vigor (or Reverb holding it) leaves the base untouched.
    [InlineData(10, 3, 5, 0, 25)]
    public void ComputeFinisherBase_CancelsTheVigorTheStrikesConsume(
        int prior, int strikes, int perStrike, int vigor, int expected)
    {
        Assert.Equal(expected, (int)FateKnocking.ComputeFinisherBase(prior, strikes, perStrike, vigor));
    }

    // Damage lands per hit and is truncated to an int on application (Creature.LoseHpInternal), so a
    // fractional per-strike number is truncated three times over, not once at the end. Extrapolating the
    // raw fraction previewed damage the card could never collect.
    [Theory]
    [InlineData(3, 4.5, 12)]   // 4+4+4, not 13.5
    [InlineData(3, 3.99, 9)]   // 3+3+3, not 11.97
    [InlineData(3, 4.0, 12)]   // already whole: unchanged
    [InlineData(3, 0.5, 0)]    // every hit truncates away to nothing
    [InlineData(3, -2.5, 0)]   // damage is clamped at 0 on application, never negative
    public void ExpectedStrikeTotal_TruncatesPerHitNotAtTheEnd(int strikes, decimal perStrike, int expected)
    {
        Assert.Equal(expected, (int)FateKnocking.ExpectedStrikeTotal(strikes, perStrike));
    }

    // The exact play that exposed this: base 1 + Tuned 2 + Vigor 3 = 6 raw, x0.75 Weak = 4.50 per strike.
    // The strikes dealt 4+4+4 = 12 and the finisher rolled (12 + 2) * 0.75 = 10.5 -> 10, while the preview
    // read (13.5 - 3 + 2 + 3) * 0.75 = 11.625 -> 11. Truncating first makes the previewed base agree.
    [Fact]
    public void ComputeFinisherBase_MatchesTheStrikesThatWillActuallyLand()
    {
        // 3 strikes previewing at 4.50 each, minus the Vigor 3 the strikes consume.
        decimal previewBase = FateKnocking.ComputeFinisherBase(0, 3, 4.5m, 3m);

        // 12 (what the hits really total), not 13.5 (the extrapolation).
        Assert.Equal(9m, previewBase);
        // Re-adding Tuned 2 and Vigor 3 the way ModifyDamage does, then Weak: the same 10.5 the finisher
        // rolled, which truncates to the 10 the player actually saw land.
        Assert.Equal(10, (int)((previewBase + 2m + 3m) * 0.75m));
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
