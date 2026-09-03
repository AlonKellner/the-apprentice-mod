using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Ensures every gold-highlighted keyword/power name mentioned in a card's cards.json description
// has a matching WithTip(...) hover tip registered on that card's constructor, and vice versa —
// no keyword mentioned without its explainer tooltip, and no orphan tip for something the card's
// own text never mentions. Works by scanning source/localization text directly (no ModelDb
// needed), the same file-scanning approach TheUnderstudyCardPoolTests.CountBCardsByRarity uses.
public class CardTooltipKeywordSyncTests
{
    // Maps a gold-tag term (as it appears in cards.json, case-insensitive) to the exact substring
    // a WithTip(...) call for it must contain somewhere in the card's source file.
    private static readonly Dictionary<string, string> TermToTipSubstring = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Weak"] = "typeof(WeakPower)",
        ["Vulnerable"] = "typeof(VulnerablePower)",
        ["Shaken"] = "typeof(ShakenPower)",
        ["Limited"] = "typeof(LimitedPower)",
        ["Jaded"] = "typeof(JadedPower)",
        ["Tension"] = "typeof(TensionPower)",
        ["Vigor"] = "typeof(VigorPower)",
        ["Planned"] = "UnderstudyKeywords.Planned)",
        ["Tuned"] = "UnderstudyKeywords.Tuned)",
        ["Invert"] = "UnderstudyKeywords.Invert)",
        ["Invertible"] = "UnderstudyKeywords.Invertible)",
        ["Swap"] = "UnderstudyKeywords.Swap)",
        ["Swappable"] = "UnderstudyKeywords.Swappable)",
        ["Unplayable"] = "CardKeyword.Unplayable)",
        ["Rewarded"] = "typeof(RewardedPower)",
        ["Punished"] = "typeof(PunishedPower)",
        ["Unweak"] = "typeof(UnweakPower)",
        ["Unvulnerable"] = "typeof(UnvulnerablePower)",
        ["Unshaken"] = "typeof(UnshakenPower)",
        ["Unjaded"] = "typeof(UnjadedPower)",
        ["Unlimited"] = "typeof(UnlimitedPower)",
        ["Stable"] = "UnderstudyKeywords.Stable)",
        // Replay is a base-game card keyword with no CardKeyword enum value and no Power — its explainer
        // is the static hover tip StaticHoverTip.ReplayStatic (same one base-game HiddenGem uses when it
        // grants Replay to another card). A card that only mentions Replay must add that tip explicitly.
        ["Replay"] = "StaticHoverTip.ReplayStatic",
    };

    // Gold-highlighted terms that are just prose emphasis, not mechanics with their own tip
    // ("Block" is assumed universally understood; "debuff" is generic English, not a specific
    // keyword/power name).
    private static readonly HashSet<string> ExcludedTerms = new(StringComparer.OrdinalIgnoreCase) { "Block", "debuff" };

    // Order/Orders (The Second Lesson's per-card affliction) has no ModelDb-backed type BaseLib's
    // WithTip(...) can resolve — AfflictionModel isn't PowerModel/CardModel/PotionModel/
    // EnchantmentModel — so it can never satisfy TermToTipSubstring. Instead it's tagged [red]
    // rather than [gold] to visually mark it as "no hover tip available", and that tagging is
    // enforced by Card_OrderMentions_AreTaggedRedNotGold below.
    private static readonly Regex GoldTagPattern = new(@"\[gold\](.*?)\[/gold\]", RegexOptions.Compiled);
    private static readonly Regex RedTagPattern = new(@"\[red\](.*?)\[/red\]", RegexOptions.Compiled);
    // Capital-O only: distinguishes the Order keyword/affliction from ordinary English word "order"
    // (e.g. Remix's "in a random order").
    private static readonly Regex OrderWordPattern = new(@"\bOrders?\b", RegexOptions.Compiled);

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

    private static string ToScreamingSnakeCase(string pascalCase)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < pascalCase.Length; i++)
        {
            char c = pascalCase[i];
            if (i > 0 && char.IsUpper(c))
            {
                bool afterLower = char.IsLower(pascalCase[i - 1]);
                // Handles single-letter words inside a run of capitals, e.g. "TakeABreath" ->
                // TAKE_A_BREATH: split between the run's last capital and the following word.
                bool endOfAcronymRun = char.IsUpper(pascalCase[i - 1]) && i + 1 < pascalCase.Length && char.IsLower(pascalCase[i + 1]);
                if (afterLower || endOfAcronymRun)
                    sb.Append('_');
            }
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }

    public static IEnumerable<object[]> AllCardFiles()
    {
        var cardsDir = Path.Combine(RepoRoot, "TheUnderstudyCode", "Cards");
        var skip = new HashSet<string> { "UnderstudyCard", "PlayAllPlannedCard" };
        // Also recognize the abstract resolver base (Curtain Call/DaCapo/Remix declare ": PlayAllPlannedCard").
        var bCardPattern = new Regex(@":\s*(?:UnderstudyCard|PlayAllPlannedCard)\b");
        foreach (var file in Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (skip.Contains(name)) continue;
            var text = File.ReadAllText(file);
            if (!bCardPattern.IsMatch(text)) continue;
            yield return new object[] { file };
        }
    }

    [Theory]
    [MemberData(nameof(AllCardFiles))]
    public void Card_TooltipsMatchDescriptionKeywords_Bidirectionally(string filePath)
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        string text = File.ReadAllText(filePath);

        var descriptions = LoadDescriptions();
        string key = "THEUNDERSTUDY-" + ToScreamingSnakeCase(className);
        Assert.True(descriptions.TryGetValue(key, out var description),
            $"{className}: no cards.json description found for key '{key}.description'");

        var mentionedTerms = new HashSet<string>(
            GoldTagPattern.Matches(description!).Select(m => m.Groups[1].Value),
            StringComparer.OrdinalIgnoreCase);
        mentionedTerms.ExceptWith(ExcludedTerms);

        var mismatches = new List<string>();
        foreach (var (term, tipSubstring) in TermToTipSubstring)
        {
            bool mentioned = mentionedTerms.Contains(term);
            bool tipped = text.Contains(tipSubstring);
            if (mentioned && !tipped)
                mismatches.Add($"description mentions [gold]{term}[/gold] but has no WithTip({tipSubstring}");
            else if (tipped && !mentioned)
                mismatches.Add($"has WithTip({tipSubstring} but description never mentions [gold]{term}[/gold]");
        }

        Assert.True(mismatches.Count == 0, $"{className}: " + string.Join("; ", mismatches));
    }

    // Comprehensive guard, and the reason this file exists at all: EVERY gold-highlighted keyword in a
    // card's description — mod OR base game — must have a matching hover tip on that card. The
    // bidirectional test above only iterates the curated TermToTipSubstring map, so a gold term that
    // isn't in the map is silently ignored — which is how Master Form shipped mentioning [gold]Replay[/gold]
    // (a base-game keyword) with no Replay tooltip. This test instead walks the actual gold terms in the
    // description and fails on ANY that isn't either excluded prose or a recognized-and-tipped keyword.
    // Adding a new keyword to a card therefore forces you to either register it here with the source
    // substring proving its tip, or mark it excluded — the term can never slip through untipped.
    [Theory]
    [MemberData(nameof(AllCardFiles))]
    public void Card_EveryGoldKeyword_HasATooltip(string filePath)
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        string text = File.ReadAllText(filePath);

        var descriptions = LoadDescriptions();
        string key = "THEUNDERSTUDY-" + ToScreamingSnakeCase(className);
        if (!descriptions.TryGetValue(key, out var description)) return; // missing-description is asserted above

        var goldTerms = new HashSet<string>(
            GoldTagPattern.Matches(description!).Select(m => m.Groups[1].Value),
            StringComparer.OrdinalIgnoreCase);

        var problems = new List<string>();
        foreach (var term in goldTerms)
        {
            if (ExcludedTerms.Contains(term)) continue;
            // Order/Orders has no tip-able backing type; Card_OrderMentions_AreTaggedRedNotGold owns
            // enforcing it is [red], not [gold], so don't double-report it here.
            if (OrderWordPattern.IsMatch(term)) continue;

            if (!TermToTipSubstring.TryGetValue(term, out var tipSubstring))
                problems.Add($"[gold]{term}[/gold] is not a recognized keyword — add a hover tip for it " +
                             $"and register '{term}' in TermToTipSubstring (or add it to ExcludedTerms if it's plain emphasis)");
            else if (!text.Contains(tipSubstring))
                problems.Add($"[gold]{term}[/gold] has no matching hover tip (expected the card source to contain \"{tipSubstring}\")");
        }

        Assert.True(problems.Count == 0, $"{className}: " + string.Join("; ", problems));
    }

    // Order/Orders must always be tagged [red], never [gold] — it's the one term with no
    // WithTip(...)-able backing type (see comment on RedTagPattern above), so [red] is how the
    // description signals "this won't show a hover tip". Applies to every Lesson card (and any
    // future card) that mentions the word, not just The Second Lesson.
    [Theory]
    [MemberData(nameof(AllCardFiles))]
    public void Card_OrderMentions_AreTaggedRedNotGold(string filePath)
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        var descriptions = LoadDescriptions();
        string key = "THEUNDERSTUDY-" + ToScreamingSnakeCase(className);
        if (!descriptions.TryGetValue(key, out var description)) return;

        var goldTerms = GoldTagPattern.Matches(description!).Select(m => m.Groups[1].Value);
        Assert.True(goldTerms.All(t => !OrderWordPattern.IsMatch(t)),
            $"{className}: description tags Order/Orders as [gold] instead of [red]");

        if (!OrderWordPattern.IsMatch(description!)) return;

        var redTerms = RedTagPattern.Matches(description!).Select(m => m.Groups[1].Value);
        Assert.True(redTerms.Any(t => OrderWordPattern.IsMatch(t)),
            $"{className}: description mentions Order/Orders but not wrapped in [red]...[/red]");
    }
}
