using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests.Timeline;

// Guardrails on what each Timeline epoch unlocks. Nothing checked this before, which is how three
// Common cards and a Common relic ended up gated, and how the epoch->content mapping drifted away from
// the story without anyone noticing.
//
// Everything here reads SOURCE rather than models: the epochs' static lists call ModelDb.Card<T>() etc.,
// and the bare test host has no ModelDb (same constraint TheUnderstudyCardPoolTests works around for its
// rarity counts). So the file is parsed for `ModelDb.Card<X>()` entries per epoch class, and rarities
// are read out of each card/relic/potion's own source.
public class EpochUnlockTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static string EpochsSource =>
        File.ReadAllText(Path.Combine(RepoRoot, "TheUnderstudyCode", "Timeline", "UnderstudyEpochs.cs"));

    // kind -> "Card" | "Relic" | "Potion" | "Event"
    private sealed record Unlock(string Epoch, string Kind, string TypeName);

    // Split the file on class declarations, then pull every ModelDb.X<Y>() out of each epoch's block.
    private static IReadOnlyList<Unlock> ParseUnlocks()
    {
        var source = EpochsSource;
        var classes = Regex.Matches(source, @"public sealed class (Understudy\d+Epoch)\b").ToList();
        var results = new List<Unlock>();

        for (int i = 0; i < classes.Count; i++)
        {
            int start = classes[i].Index;
            int end = i + 1 < classes.Count ? classes[i + 1].Index : source.Length;
            string body = source[start..end];

            foreach (Match m in Regex.Matches(body, @"ModelDb\.(Card|Relic|Potion|Event)<(\w+)>\(\)"))
                results.Add(new Unlock(classes[i].Groups[1].Value, m.Groups[1].Value, m.Groups[2].Value));
        }

        Assert.NotEmpty(results); // the parse itself must not silently return nothing
        return results;
    }

    // The rarity declared in a card/relic/potion's own constructor, e.g. `CardRarity.Rare`.
    private static string RarityOf(string kind, string typeName)
    {
        string dir = kind switch
        {
            "Card" => "Cards",
            "Relic" => "Relics",
            "Potion" => "Potions",
            _ => throw new ArgumentException($"no rarity concept for {kind}", nameof(kind)),
        };

        var file = Directory
            .GetFiles(Path.Combine(RepoRoot, "TheUnderstudyCode", dir), typeName + ".cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        Assert.True(file != null, $"No source file found for {kind} '{typeName}'.");

        var m = Regex.Match(File.ReadAllText(file!), $@"{kind}Rarity\.(\w+)");
        Assert.True(m.Success, $"No {kind}Rarity found in {typeName}.cs.");
        return m.Groups[1].Value;
    }

    // THE rule: unlocking should feel like gaining a new option, not like the basics were withheld.
    // Commons are the connective tissue of a deck and must be available from the first run — for CARDS and
    // RELICS. Potions are exempt: the whole roster is only 3 (one per rarity), so the "Consumed" epoch
    // gates all of them, Common included, and there is no connective-tissue concern for a 3-potion set.
    [Fact]
    public void NoCommonCardOrRelicIsEpochGated()
    {
        var commons = ParseUnlocks()
            .Where(u => u.Kind != "Event" && u.Kind != "Potion")
            .Where(u => RarityOf(u.Kind, u.TypeName) == "Common")
            .Select(u => $"{u.Epoch} gates Common {u.Kind.ToLowerInvariant()} {u.TypeName}")
            .ToList();

        Assert.True(commons.Count == 0,
            "Only Uncommon/Rare cards & relics may be epoch-gated:\n  " + string.Join("\n  ", commons));
    }

    // EpochModel.CreateCardUnlockText / CreateRelicUnlockText / CreatePotionUnlockText all loop
    // `for (i = 0; i < 3; i++)` and index [i], so a list of any other length throws at unlock time —
    // on the Timeline screen, where it is most painful to discover.
    [Fact]
    public void CardRelicAndPotionEpochsUnlockExactlyThree()
    {
        var offenders = ParseUnlocks()
            .Where(u => u.Kind != "Event")
            .GroupBy(u => u.Epoch)
            .Where(g => g.Count() != 3)
            .Select(g => $"{g.Key} unlocks {g.Count()} ({string.Join(", ", g.Select(u => u.TypeName))})")
            .ToList();

        Assert.True(offenders.Count == 0,
            "The base CreateXUnlockText helpers index [0..2] and require exactly 3:\n  "
            + string.Join("\n  ", offenders));
    }

    // Event epochs follow the base game's Event1Epoch shape: one event, its own unlockText loc key, and
    // QueueMiscUnlock — deliberately NOT the 3-item card/relic/potion helpers.
    [Fact]
    public void EventEpochUnlocksExactlyOneEventAndUsesTheMiscQueue()
    {
        var events = ParseUnlocks().Where(u => u.Kind == "Event").ToList();
        Assert.Single(events);

        var epoch = events[0].Epoch;
        var body = EpochBody(epoch);
        Assert.Contains("QueueMiscUnlock", body);
        Assert.DoesNotContain("CreateCardUnlockText", body);
    }

    // An item gated twice would be unlocked by whichever epoch reveals first, making the second epoch's
    // advertised unlock a lie.
    [Fact]
    public void NoContentIsGatedByTwoEpochs()
    {
        var dupes = ParseUnlocks()
            .GroupBy(u => (u.Kind, u.TypeName))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.TypeName} gated by {string.Join(" and ", g.Select(u => u.Epoch))}")
            .ToList();

        Assert.True(dupes.Count == 0, "Content gated by more than one epoch:\n  " + string.Join("\n  ", dupes));
    }

    // The lists live in UnderstudyEpochs.cs but the actual pruning lives in three different pool files,
    // each using a different extension point. Moving a list without moving its pool gate silently leaves
    // the content permanently locked (or permanently free) — this is the check that catches it.
    [Theory]
    [InlineData("Card", "TheUnderstudyCardPool.cs")]
    [InlineData("Relic", "TheUnderstudyRelicPool.cs")]
    [InlineData("Potion", "TheUnderstudyPotionPool.cs")]
    public void EachPoolGatesExactlyTheEpochsOwningThatContent(string kind, string poolFile)
    {
        var owning = ParseUnlocks().Where(u => u.Kind == kind).Select(u => u.Epoch).Distinct().OrderBy(e => e).ToList();

        var pool = File.ReadAllText(Path.Combine(RepoRoot, "TheUnderstudyCode", "Character", poolFile));
        var referenced = Regex.Matches(pool, @"\bUnderstudy(\d+)Epoch\b")
            .Select(m => "Understudy" + m.Groups[1].Value + "Epoch")
            .Distinct().OrderBy(e => e).ToList();

        Assert.Equal(owning, referenced);
    }

    private static string EpochBody(string epochClass)
    {
        var source = EpochsSource;
        var classes = Regex.Matches(source, @"public sealed class (Understudy\d+Epoch)\b").ToList();
        for (int i = 0; i < classes.Count; i++)
        {
            if (classes[i].Groups[1].Value != epochClass) continue;
            int end = i + 1 < classes.Count ? classes[i + 1].Index : source.Length;
            return source[classes[i].Index..end];
        }
        throw new InvalidOperationException($"epoch class {epochClass} not found");
    }
}
