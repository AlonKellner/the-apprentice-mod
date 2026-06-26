using System.Text.Json;
using Xunit;

namespace TheApprentice.Tests;

public class LocalizationTests
{
    private static Dictionary<string, string> LoadJson(string relativePath)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var path = Path.Combine(root, relativePath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
    }

    [Fact]
    public void CardKeywordsJson_HasPlannedTitle()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/card_keywords.json");
        Assert.True(dict.ContainsKey("THEAPPRENTICE-PLANNED.title"), "Missing key: THEAPPRENTICE-PLANNED.title");
        Assert.False(string.IsNullOrWhiteSpace(dict["THEAPPRENTICE-PLANNED.title"]));
    }

    [Fact]
    public void CardKeywordsJson_HasPlannedDescription()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/card_keywords.json");
        Assert.True(dict.ContainsKey("THEAPPRENTICE-PLANNED.description"), "Missing key: THEAPPRENTICE-PLANNED.description");
        Assert.False(string.IsNullOrWhiteSpace(dict["THEAPPRENTICE-PLANNED.description"]));
    }

    [Fact]
    public void CardsJson_HasAllPlannedCardKeys()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/cards.json");
        string[] expectedKeys = [
            "THEAPPRENTICE-CONTEMPLATE.title", "THEAPPRENTICE-CONTEMPLATE.description", "THEAPPRENTICE-CONTEMPLATE+.description", "THEAPPRENTICE-CONTEMPLATE.selectionPrompt",
            "THEAPPRENTICE-REALIZE.title", "THEAPPRENTICE-REALIZE.description", "THEAPPRENTICE-REALIZE+.description", "THEAPPRENTICE-REALIZE.selectionPrompt",
            "THEAPPRENTICE-EPIPHANY.title", "THEAPPRENTICE-EPIPHANY.description", "THEAPPRENTICE-EPIPHANY+.description", "THEAPPRENTICE-EPIPHANY.selectionPrompt",
            "THEAPPRENTICE-GROOVE.title", "THEAPPRENTICE-GROOVE.description", "THEAPPRENTICE-GROOVE+.description",
            "THEAPPRENTICE-OVERTHINKING.title", "THEAPPRENTICE-OVERTHINKING.description", "THEAPPRENTICE-OVERTHINKING+.description",
            "THEAPPRENTICE-CREATIVE_BLOCK.title", "THEAPPRENTICE-CREATIVE_BLOCK.description", "THEAPPRENTICE-CREATIVE_BLOCK+.description",
            "THEAPPRENTICE-REHEARSAL.title", "THEAPPRENTICE-REHEARSAL.description", "THEAPPRENTICE-REHEARSAL+.description", "THEAPPRENTICE-REHEARSAL.selectionPrompt",
            "THEAPPRENTICE-CLEAR_MIND.title", "THEAPPRENTICE-CLEAR_MIND.description", "THEAPPRENTICE-CLEAR_MIND+.description",
            "THEAPPRENTICE-TABULA_RASA.title", "THEAPPRENTICE-TABULA_RASA.description", "THEAPPRENTICE-TABULA_RASA+.description",
            "THEAPPRENTICE-TRANSPOSE.title", "THEAPPRENTICE-TRANSPOSE.description", "THEAPPRENTICE-TRANSPOSE+.description",
            "THEAPPRENTICE-TRANSPOSE.selectionPrompt1", "THEAPPRENTICE-TRANSPOSE.selectionPrompt2",
            "THEAPPRENTICE-SIGNATURE.title", "THEAPPRENTICE-SIGNATURE.description", "THEAPPRENTICE-SIGNATURE+.description",
            "THEAPPRENTICE-PRELUDE.title", "THEAPPRENTICE-PRELUDE.description", "THEAPPRENTICE-PRELUDE+.description",
            "THEAPPRENTICE-SCHEMING.title", "THEAPPRENTICE-SCHEMING.description", "THEAPPRENTICE-SCHEMING+.description", "THEAPPRENTICE-SCHEMING.selectionPrompt",
            "THEAPPRENTICE-IN_THE_ZONE.title", "THEAPPRENTICE-IN_THE_ZONE.description", "THEAPPRENTICE-IN_THE_ZONE+.description",
            "THEAPPRENTICE-OBSESSION.title", "THEAPPRENTICE-OBSESSION.description", "THEAPPRENTICE-OBSESSION+.description",
            "THEAPPRENTICE-ENCORE.title", "THEAPPRENTICE-ENCORE.description", "THEAPPRENTICE-ENCORE+.description",
            "THEAPPRENTICE-VIRTUOSO.title", "THEAPPRENTICE-VIRTUOSO.description", "THEAPPRENTICE-VIRTUOSO+.description",
            "THEAPPRENTICE-MAGNUM_OPUS.title", "THEAPPRENTICE-MAGNUM_OPUS.description", "THEAPPRENTICE-MAGNUM_OPUS+.description", "THEAPPRENTICE-MAGNUM_OPUS.selectionPrompt",
        ];
        var missing = expectedKeys.Where(k => !dict.ContainsKey(k) || string.IsNullOrWhiteSpace(dict[k])).ToList();
        Assert.True(missing.Count == 0, $"Missing or empty localization keys:\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void CardsJson_HasAllDreamsAndAmbitionsCardKeys()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/cards.json");
        string[] expectedKeys = [
            "THEAPPRENTICE-DREAM.title", "THEAPPRENTICE-DREAM.description", "THEAPPRENTICE-DREAM+.description",
            "THEAPPRENTICE-AMBITION.title", "THEAPPRENTICE-AMBITION.description", "THEAPPRENTICE-AMBITION+.description",
            "THEAPPRENTICE-POTENTIAL.title", "THEAPPRENTICE-POTENTIAL.description", "THEAPPRENTICE-POTENTIAL+.description",
            "THEAPPRENTICE-NOCTURNE.title", "THEAPPRENTICE-NOCTURNE.description", "THEAPPRENTICE-NOCTURNE+.description",
            "THEAPPRENTICE-HUBRIS.title", "THEAPPRENTICE-HUBRIS.description", "THEAPPRENTICE-HUBRIS+.description",
            "THEAPPRENTICE-LONGING.title", "THEAPPRENTICE-LONGING.description", "THEAPPRENTICE-LONGING+.description",
            "THEAPPRENTICE-DRIVE.title", "THEAPPRENTICE-DRIVE.description", "THEAPPRENTICE-DRIVE+.description",
            "THEAPPRENTICE-REVERIE.title", "THEAPPRENTICE-REVERIE.description", "THEAPPRENTICE-REVERIE+.description",
            "THEAPPRENTICE-SUBLIMATION.title", "THEAPPRENTICE-SUBLIMATION.description", "THEAPPRENTICE-SUBLIMATION+.description",
            "THEAPPRENTICE-SYNTHESIS.title", "THEAPPRENTICE-SYNTHESIS.description", "THEAPPRENTICE-SYNTHESIS+.description", "THEAPPRENTICE-SYNTHESIS.selectionPrompt",
            "THEAPPRENTICE-DRIVEN_INSPIRATION.title", "THEAPPRENTICE-DRIVEN_INSPIRATION.description", "THEAPPRENTICE-DRIVEN_INSPIRATION+.description", "THEAPPRENTICE-DRIVEN_INSPIRATION.selectionPrompt",
            "THEAPPRENTICE-LULLABY.title", "THEAPPRENTICE-LULLABY.description", "THEAPPRENTICE-LULLABY+.description",
            "THEAPPRENTICE-CONVICTION.title", "THEAPPRENTICE-CONVICTION.description", "THEAPPRENTICE-CONVICTION+.description",
            "THEAPPRENTICE-MANIFESTO.title", "THEAPPRENTICE-MANIFESTO.description", "THEAPPRENTICE-MANIFESTO+.description",
            "THEAPPRENTICE-LUCIDITY.title", "THEAPPRENTICE-LUCIDITY.description", "THEAPPRENTICE-LUCIDITY+.description",
            "THEAPPRENTICE-RESOLVE.title", "THEAPPRENTICE-RESOLVE.description", "THEAPPRENTICE-RESOLVE+.description",
            "THEAPPRENTICE-IGNITION.title", "THEAPPRENTICE-IGNITION.description", "THEAPPRENTICE-IGNITION+.description",
            "THEAPPRENTICE-CHRYSALIS.title", "THEAPPRENTICE-CHRYSALIS.description", "THEAPPRENTICE-CHRYSALIS+.description",
            "THEAPPRENTICE-DAYDREAM.title", "THEAPPRENTICE-DAYDREAM.description", "THEAPPRENTICE-DAYDREAM+.description",
            "THEAPPRENTICE-WUNDERKIND.title", "THEAPPRENTICE-WUNDERKIND.description", "THEAPPRENTICE-WUNDERKIND+.description",
            "THEAPPRENTICE-PRODIGY.title", "THEAPPRENTICE-PRODIGY.description", "THEAPPRENTICE-PRODIGY+.description",
            "THEAPPRENTICE-CONDUCTOR.title", "THEAPPRENTICE-CONDUCTOR.description", "THEAPPRENTICE-CONDUCTOR+.description",
            "THEAPPRENTICE-BLUEPRINT.title", "THEAPPRENTICE-BLUEPRINT.description", "THEAPPRENTICE-BLUEPRINT+.description",
        ];
        var missing = expectedKeys.Where(k => !dict.ContainsKey(k) || string.IsNullOrWhiteSpace(dict[k])).ToList();
        Assert.True(missing.Count == 0, $"Missing or empty D&A localization keys:\n{string.Join("\n", missing)}");
    }

}
