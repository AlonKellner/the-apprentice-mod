using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheUnderstudy.Tests.Art;

// One art slot the mod can fill: where the file lives, what it is called, and which content id owns it.
public sealed record ArtSlot(string Category, string Id, string Slug, string RelativePath)
{
    public override string ToString() => RelativePath;
}

// Filesystem view of the mod's art, derived from the shipping localization files. Deliberately no
// ResourceLoader, no ModelDb and no Log.* — any of those crashes the bare xUnit host — so this reads
// the same JSON the game ships and the same folders the pack exports, and nothing else.
//
// The naming contract mirrored here is the one in ART.md and implemented by StringExtensions:
// slug = the loc id minus the "THEUNDERSTUDY-" prefix, lowercased.
public static class ArtCoverage
{
    public const string IdPrefix = "THEUNDERSTUDY-";

    public static readonly string ImagesDir = FindImagesDir();

    private static string FindImagesDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "TheUnderstudy", "images");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate TheUnderstudy/images above " + AppContext.BaseDirectory);
    }

    public static string Slug(string locId) => locId[IdPrefix.Length..].ToLowerInvariant();

    private static IEnumerable<string> Ids(string table) =>
        LocText.Table(table).Keys
            .Where(k => k.StartsWith(IdPrefix, StringComparison.Ordinal))
            .Select(k => k.Split('.')[0])
            .Distinct()
            .OrderBy(k => k, StringComparer.Ordinal);

    // Powers with no icon of their own, by design.
    //   INVERT_TRACKER_POWER / TUNED_LOCK_POWER — always-hidden observer powers, never shown to the
    //     player (same two LocFileTests exempts from its blank-description check).
    //   TUNED_COUNTER_POWER — deliberately aliased to the Tuned icon by TunedCounterPower's own
    //     override, so it must never be counted as needing art of its own.
    public static readonly HashSet<string> PowersWithoutOwnIcon = new()
    {
        "THEUNDERSTUDY-INVERT_TRACKER_POWER",
        "THEUNDERSTUDY-TUNED_LOCK_POWER",
        "THEUNDERSTUDY-TUNED_COUNTER_POWER",
    };

    // Files that live in an art folder without belonging to a content id — shared placeholders and
    // hand-addressed aliases. Excluded from the orphan check, which otherwise reads them as art for a
    // deleted card.
    //
    // tuned_power.png is the alias target: TunedCounterPower names it directly rather than deriving it
    // from its own slug, so no id claims the file. It is listed here for exactly the same reason
    // TUNED_COUNTER_POWER is listed above — the two entries are one decision, and changing that aliasing
    // means changing both.
    public static readonly HashSet<string> NonContentFiles = new(StringComparer.Ordinal)
    {
        "card_portraits/placeholders/attack.png",
        "card_portraits/placeholders/skill.png",
        "card_portraits/placeholders/power.png",
        "powers/tuned_power.png",
        "powers/big/tuned_power.png",
    };

    // Every slot the mod could fill, whether or not the file exists yet.
    public static IReadOnlyList<ArtSlot> AllSlots()
    {
        var slots = new List<ArtSlot>();

        foreach (var id in Ids("cards"))
        {
            var slug = Slug(id);
            slots.Add(new ArtSlot("card", id, slug, $"card_portraits/{slug}.png"));
        }

        foreach (var id in Ids("powers").Where(i => !PowersWithoutOwnIcon.Contains(i)))
        {
            var slug = Slug(id);
            slots.Add(new ArtSlot("power", id, slug, $"powers/{slug}.png"));
            slots.Add(new ArtSlot("power-big", id, slug, $"powers/big/{slug}.png"));
        }

        foreach (var id in Ids("relics"))
        {
            var slug = Slug(id);
            slots.Add(new ArtSlot("relic", id, slug, $"relics/{slug}.png"));
            slots.Add(new ArtSlot("relic-outline", id, slug, $"relics/outline/{slug}.png"));
            slots.Add(new ArtSlot("relic-big", id, slug, $"relics/big/{slug}.png"));
        }

        foreach (var id in Ids("potions"))
        {
            var slug = Slug(id);
            slots.Add(new ArtSlot("potion", id, slug, $"potions/{slug}.png"));
            slots.Add(new ArtSlot("potion-outline", id, slug, $"potions/outline/{slug}.png"));
        }

        return slots;
    }

    public static bool Exists(ArtSlot slot) =>
        File.Exists(Path.Combine(ImagesDir, slot.RelativePath.Replace('/', Path.DirectorySeparatorChar)));

    // Every image actually on disk, as a forward-slash path relative to TheUnderstudy/images.
    public static IReadOnlyList<string> ShippedImages() =>
        Directory.EnumerateFiles(ImagesDir, "*.png", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(ImagesDir, p).Replace(Path.DirectorySeparatorChar, '/'))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
}
