using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TheUnderstudy.Tests.Art;

// Guards the art pipeline rather than the art. Three things can go wrong between a filename in the
// commission brief and a texture on screen, and none of them fails the build on its own:
//
//   1. A card/power/relic/potion is renamed and its art file keeps the old name. The path resolvers all
//      fall back silently, so the card just quietly loses its portrait. renaming.md shows names have
//      churned heavily here — this is the failure mode most likely to actually happen.
//   2. Art is delivered under a wrong or misspelled name and never loads.
//   3. Art that used to resolve stops resolving because a folder convention moved.
//
// It does NOT assert that all ~280 assets exist: they don't yet, and a red suite would just get
// ignored. Coverage is measured against a committed baseline instead, so the suite passes today, fails
// if coverage regresses, and prints progress as art lands.
public class ArtCoverageTests(ITestOutputHelper output)
{
    private static readonly string BaselinePath = FindBaseline();

    private static string FindBaseline()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "art", "coverage-baseline.json");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Could not locate art/coverage-baseline.json above " + AppContext.BaseDirectory);
    }

    private static Dictionary<string, int> Baseline() =>
        JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(BaselinePath))
        ?? throw new InvalidOperationException("coverage-baseline.json did not parse into an object.");

    // The anti-rename guard, and the reason this file exists. Any bespoke art file sitting in a content
    // folder must belong to a live content id. Rename the card without renaming its portrait and the
    // portrait lands here as an orphan — instead of silently vanishing from the card.
    [Fact]
    public void EveryShippedArtFile_BelongsToALiveContentId()
    {
        var expected = ArtCoverage.AllSlots().Select(s => s.RelativePath).ToHashSet(StringComparer.Ordinal);

        // Folders whose contents are keyed to content ids. Everything else (charui, restsite, shared
        // outlines) is addressed by hand from code and has no id to check against.
        string[] contentFolders = ["card_portraits/", "powers/", "relics/", "potions/"];

        var orphans = ArtCoverage.ShippedImages()
            .Where(p => contentFolders.Any(f => p.StartsWith(f, StringComparison.Ordinal)))
            .Where(p => !ArtCoverage.NonContentFiles.Contains(p))
            .Where(p => !expected.Contains(p))
            .ToList();

        Assert.True(orphans.Count == 0,
            "Art files that match no live content id — a rename probably orphaned them. Either rename the " +
            "file to the new slug or delete it:\n  " + string.Join("\n  ", orphans));
    }

    // Coverage must never go backwards. A drop means art was deleted, or content was renamed and its
    // art left behind; both are regressions worth failing on.
    [Fact]
    public void ArtCoverage_DoesNotRegress()
    {
        var baseline = Baseline();
        var have = ArtCoverage.AllSlots()
            .Where(ArtCoverage.Exists)
            .GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var regressions = baseline
            .Select(kv => (kv.Key, Expected: kv.Value, Actual: have.GetValueOrDefault(kv.Key, 0)))
            .Where(x => x.Actual < x.Expected)
            .Select(x => $"{x.Key}: {x.Actual} present, baseline expects at least {x.Expected}")
            .ToList();

        Assert.True(regressions.Count == 0,
            "Art coverage regressed:\n  " + string.Join("\n  ", regressions) +
            "\nIf this was intentional, lower the numbers in art/coverage-baseline.json.");
    }

    // Not an assertion — a progress bar. `dotnet test -v n` prints this, which is how the art backlog
    // gets tracked without maintaining a count by hand anywhere.
    [Fact]
    public void ReportCoverage()
    {
        var slots = ArtCoverage.AllSlots();
        output.WriteLine("Art coverage (see ART.md and art/assets/ for the specs)");
        output.WriteLine("");

        foreach (var group in slots.GroupBy(s => s.Category).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            int have = group.Count(ArtCoverage.Exists);
            int total = group.Count();
            int pct = total == 0 ? 100 : have * 100 / total;
            output.WriteLine($"  {group.Key,-14} {have,3}/{total,-3}  {pct,3}%");
        }

        int allHave = slots.Count(ArtCoverage.Exists);
        output.WriteLine("");
        output.WriteLine($"  {"TOTAL",-14} {allHave,3}/{slots.Count,-3}  {(slots.Count == 0 ? 100 : allHave * 100 / slots.Count),3}%");

        Assert.NotEmpty(slots);
    }
}
