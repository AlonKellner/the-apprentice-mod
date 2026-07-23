using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.Tests;

// Player-facing text lives in TheUnderstudy/localization/eng/*.json, not in inline Localization
// properties, so tests read it back from the files that actually ship. Reading the real files (rather
// than a fixture) means these assertions fail if someone edits the shipping text into a wrong state,
// which is the whole point of keeping them.
//
// Of() returns the same (suffix, text) shape the old inline declarations returned, in the same
// canonical order (title, description, then smartDescription for powers / flavor for relics), so the
// assertions written against those declarations still read naturally and can still index by position.
public static class LocText
{
    private static readonly string Dir = FindLocDir();
    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new();

    // Walk up from the test binary until the localization folder appears. Keeps the tests runnable
    // from any working directory without hard-coding a path or copying the JSON into the output.
    private static string FindLocDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "TheUnderstudy", "localization", "eng");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate TheUnderstudy/localization/eng above " + AppContext.BaseDirectory);
    }

    public static IReadOnlyDictionary<string, string> Table(string name)
    {
        lock (Tables)
        {
            if (!Tables.TryGetValue(name, out var table))
            {
                string path = Path.Combine(Dir, name + ".json");
                table = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                        ?? throw new InvalidOperationException($"{name}.json did not parse into an object.");
                Tables[name] = table;
            }
            return table;
        }
    }

    public static IEnumerable<string> TableNames() =>
        Directory.EnumerateFiles(Dir, "*.json").Select(Path.GetFileNameWithoutExtension)!;

    // Mirrors the key BaseLib derives from the model's type name (underscores at word boundaries,
    // uppercased) — the same transformation that generated the entries in the JSON files.
    public static string Slug(string typeName)
    {
        string s = Regex.Replace(typeName, "(?<=[a-z0-9])(?=[A-Z])", "_");
        s = Regex.Replace(s, "(?<=[A-Z])(?=[A-Z][a-z])", "_");
        return s.ToUpperInvariant();
    }

    public static string KeyFor(Type type) => "THEUNDERSTUDY-" + Slug(type.Name);

    public static List<(string, string)> Of(object model) => Of(model.GetType());

    public static List<(string, string)> Of(Type type)
    {
        bool isPower = typeof(PowerModel).IsAssignableFrom(type);
        var table = Table(isPower ? "powers" : "relics");
        string prefix = KeyFor(type) + ".";
        var order = isPower
            ? new[] { "title", "description", "smartDescription" }
            : new[] { "title", "description", "flavor" };

        var found = table.Where(kv => kv.Key.StartsWith(prefix, StringComparison.Ordinal))
            .ToDictionary(kv => kv.Key.Substring(prefix.Length), kv => kv.Value);

        var result = order.Where(found.ContainsKey).Select(s => (s, found[s])).ToList();
        result.AddRange(found.Where(kv => !order.Contains(kv.Key)).Select(kv => (kv.Key, kv.Value)));
        return result;
    }
}
