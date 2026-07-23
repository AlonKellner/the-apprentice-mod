using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace TheUnderstudy.Tests;

// All player-facing text ships in TheUnderstudy/localization/eng/*.json. These tests read those files
// straight from disk — no game, no ModelDb — so they guard the text itself rather than any code that
// happens to reference it. That makes them the safety net for hand-edits to the loc files.
public class LocFileTests
{
    public static IEnumerable<object[]> Tables() => LocText.TableNames().Select(n => new object[] { n });

    [Theory]
    [MemberData(nameof(Tables))]
    public void Table_ParsesAsFlatStringMap(string table)
    {
        var parsed = LocText.Table(table);
        Assert.NotNull(parsed);
    }

    // InvertTrackerPower and TunedLockPower are always-hidden observer powers auto-attached at combat
    // start; they are never shown to the player, so their description text is deliberately empty.
    // Listing them explicitly means a *new* accidental blank still fails this test.
    private static readonly HashSet<string> IntentionallyBlank = new()
    {
        "THEUNDERSTUDY-INVERT_TRACKER_POWER.description",
        "THEUNDERSTUDY-INVERT_TRACKER_POWER.smartDescription",
        "THEUNDERSTUDY-TUNED_LOCK_POWER.description",
        "THEUNDERSTUDY-TUNED_LOCK_POWER.smartDescription",
    };

    [Theory]
    [MemberData(nameof(Tables))]
    public void Table_HasNoBlankValues(string table)
    {
        var blank = LocText.Table(table)
            .Where(kv => string.IsNullOrWhiteSpace(kv.Value) && !IntentionallyBlank.Contains(kv.Key))
            .Select(kv => kv.Key).ToList();
        Assert.True(blank.Count == 0, $"{table}.json has blank values: {string.Join(", ", blank)}");
    }

    // Whatever else may be empty, a name always shows somewhere in the UI.
    [Theory]
    [MemberData(nameof(Tables))]
    public void Table_HasNoBlankTitles(string table)
    {
        var blank = LocText.Table(table)
            .Where(kv => (kv.Key.EndsWith(".title") || kv.Key.EndsWith(".name")) && string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => kv.Key).ToList();
        Assert.True(blank.Count == 0, $"{table}.json has blank titles: {string.Join(", ", blank)}");
    }

    // A duplicated key silently wins or loses depending on parser order, so catch it in the raw text
    // rather than in the parsed dictionary, which has already collapsed the duplicate.
    [Theory]
    [MemberData(nameof(Tables))]
    public void Table_HasNoDuplicateKeys(string table)
    {
        var keys = new List<string>();
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(LocDir, table + ".json")));
        foreach (var prop in doc.RootElement.EnumerateObject()) keys.Add(prop.Name);
        var dupes = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.True(dupes.Count == 0, $"{table}.json has duplicate keys: {string.Join(", ", dupes)}");
    }

    // The Order flavor lines are read at runtime by scanning flavor.0, flavor.1, ... until an index
    // is missing, so a gap would silently truncate the list rather than fail loudly.
    [Fact]
    public void OrderFlavorLines_AreContiguousFromZero()
    {
        const string prefix = "TheUnderstudy:Order.flavor.";
        var indices = LocText.Table("card_modifiers").Keys
            .Where(k => k.StartsWith(prefix))
            .Select(k => int.Parse(k.Substring(prefix.Length)))
            .OrderBy(i => i)
            .ToList();

        Assert.NotEmpty(indices);
        Assert.Equal(Enumerable.Range(0, indices.Count), indices);
    }

    [Fact]
    public void OrderCommands_ArePresent()
    {
        var table = LocText.Table("card_modifiers");
        Assert.Contains("TheUnderstudy:Order.playThis", table.Keys);
        Assert.Contains("TheUnderstudy:Order.dontPlayThis", table.Keys);
    }

    private static string LocDir
    {
        get
        {
            var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "TheUnderstudy", "localization", "eng");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("localization/eng not found");
        }
    }
}
