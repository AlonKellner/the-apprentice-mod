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
            "THEAPPRENTICE-IMPROVISE.title", "THEAPPRENTICE-IMPROVISE.description", "THEAPPRENTICE-IMPROVISE+.description",
            "THEAPPRENTICE-NEW_PERSPECTIVE.title", "THEAPPRENTICE-NEW_PERSPECTIVE.description", "THEAPPRENTICE-NEW_PERSPECTIVE+.description",
            "THEAPPRENTICE-CREATIVE_BLOCK.title", "THEAPPRENTICE-CREATIVE_BLOCK.description", "THEAPPRENTICE-CREATIVE_BLOCK+.description",
            "THEAPPRENTICE-REHEARSAL.title", "THEAPPRENTICE-REHEARSAL.description", "THEAPPRENTICE-REHEARSAL+.description",
            "THEAPPRENTICE-CLEAR_MIND.title", "THEAPPRENTICE-CLEAR_MIND.description", "THEAPPRENTICE-CLEAR_MIND+.description",
            "THEAPPRENTICE-TABULA_RASA.title", "THEAPPRENTICE-TABULA_RASA.description", "THEAPPRENTICE-TABULA_RASA+.description",
            "THEAPPRENTICE-TRANSPOSE.title", "THEAPPRENTICE-TRANSPOSE.description", "THEAPPRENTICE-TRANSPOSE+.description",
            "THEAPPRENTICE-TRANSPOSE.selectionPrompt",
            "THEAPPRENTICE-SIGNATURE.title", "THEAPPRENTICE-SIGNATURE.description", "THEAPPRENTICE-SIGNATURE+.description",
            "THEAPPRENTICE-PRELUDE.title", "THEAPPRENTICE-PRELUDE.description", "THEAPPRENTICE-PRELUDE+.description",
            "THEAPPRENTICE-SCHEMING.title", "THEAPPRENTICE-SCHEMING.description", "THEAPPRENTICE-SCHEMING+.description", "THEAPPRENTICE-SCHEMING.selectionPrompt",
            "THEAPPRENTICE-IN_THE_ZONE.title", "THEAPPRENTICE-IN_THE_ZONE.description", "THEAPPRENTICE-IN_THE_ZONE+.description",
            "THEAPPRENTICE-OBSESSION.title", "THEAPPRENTICE-OBSESSION.description", "THEAPPRENTICE-OBSESSION+.description",
            "THEAPPRENTICE-ENCORE.title", "THEAPPRENTICE-ENCORE.description", "THEAPPRENTICE-ENCORE+.description",
            "THEAPPRENTICE-VIRTUOSO.title", "THEAPPRENTICE-VIRTUOSO.description", "THEAPPRENTICE-VIRTUOSO+.description",
            "THEAPPRENTICE-MAGNUM_OPUS.title", "THEAPPRENTICE-MAGNUM_OPUS.description", "THEAPPRENTICE-MAGNUM_OPUS+.description", "THEAPPRENTICE-MAGNUM_OPUS.selectionPrompt",
            "THEAPPRENTICE-PREFACE.title", "THEAPPRENTICE-PREFACE.description", "THEAPPRENTICE-PREFACE+.description",
            "THEAPPRENTICE-LEITMOTIF.title", "THEAPPRENTICE-LEITMOTIF.description", "THEAPPRENTICE-LEITMOTIF+.description", "THEAPPRENTICE-LEITMOTIF.selectionPrompt",
            "THEAPPRENTICE-DRAFT.title", "THEAPPRENTICE-DRAFT.description", "THEAPPRENTICE-DRAFT+.description",
            "THEAPPRENTICE-FORESIGHT.title", "THEAPPRENTICE-FORESIGHT.description", "THEAPPRENTICE-FORESIGHT+.description", "THEAPPRENTICE-FORESIGHT.selectionPrompt",
            "THEAPPRENTICE-OVERTURE.title", "THEAPPRENTICE-OVERTURE.description", "THEAPPRENTICE-OVERTURE+.description",
            "THEAPPRENTICE-MAESTRO.title", "THEAPPRENTICE-MAESTRO.description", "THEAPPRENTICE-MAESTRO+.description",
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
            "THEAPPRENTICE-PASTICHE.title", "THEAPPRENTICE-PASTICHE.description", "THEAPPRENTICE-PASTICHE+.description", "THEAPPRENTICE-PASTICHE.selectionPrompt",
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
            "THEAPPRENTICE-SOLACE.title", "THEAPPRENTICE-SOLACE.description", "THEAPPRENTICE-SOLACE+.description",
            "THEAPPRENTICE-CHORUS.title", "THEAPPRENTICE-CHORUS.description", "THEAPPRENTICE-CHORUS+.description",
            "THEAPPRENTICE-WISHFUL.title", "THEAPPRENTICE-WISHFUL.description", "THEAPPRENTICE-WISHFUL+.description",
            "THEAPPRENTICE-PASSION.title", "THEAPPRENTICE-PASSION.description", "THEAPPRENTICE-PASSION+.description",
            "THEAPPRENTICE-CRESCENDO.title", "THEAPPRENTICE-CRESCENDO.description", "THEAPPRENTICE-CRESCENDO+.description",
        ];
        var missing = expectedKeys.Where(k => !dict.ContainsKey(k) || string.IsNullOrWhiteSpace(dict[k])).ToList();
        Assert.True(missing.Count == 0, $"Missing or empty D&A localization keys:\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void CardsJson_HasAllEmotionalExpressionCardKeys()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/cards.json");
        string[] expectedKeys = [
            "THEAPPRENTICE-LAMENT.title", "THEAPPRENTICE-LAMENT.description", "THEAPPRENTICE-LAMENT+.description",
            "THEAPPRENTICE-CANTICLE.title", "THEAPPRENTICE-CANTICLE.description", "THEAPPRENTICE-CANTICLE+.description",
            "THEAPPRENTICE-OUTCRY.title", "THEAPPRENTICE-OUTCRY.description", "THEAPPRENTICE-OUTCRY+.description",
            "THEAPPRENTICE-CONFESSION.title", "THEAPPRENTICE-CONFESSION.description", "THEAPPRENTICE-CONFESSION+.description",
            "THEAPPRENTICE-REPOSE.title", "THEAPPRENTICE-REPOSE.description", "THEAPPRENTICE-REPOSE+.description",
            "THEAPPRENTICE-CANDOR.title", "THEAPPRENTICE-CANDOR.description", "THEAPPRENTICE-CANDOR+.description",
            "THEAPPRENTICE-INVERSION.title", "THEAPPRENTICE-INVERSION.description", "THEAPPRENTICE-INVERSION+.description",
            "THEAPPRENTICE-REVERSAL.title", "THEAPPRENTICE-REVERSAL.description", "THEAPPRENTICE-REVERSAL+.description",
            "THEAPPRENTICE-DISCORD.title", "THEAPPRENTICE-DISCORD.description", "THEAPPRENTICE-DISCORD+.description",
            "THEAPPRENTICE-RELEASE.title", "THEAPPRENTICE-RELEASE.description", "THEAPPRENTICE-RELEASE+.description",
            "THEAPPRENTICE-TRANSFERENCE.title", "THEAPPRENTICE-TRANSFERENCE.description", "THEAPPRENTICE-TRANSFERENCE+.description", "THEAPPRENTICE-TRANSFERENCE.selectionPrompt",
            "THEAPPRENTICE-REFLECTION.title", "THEAPPRENTICE-REFLECTION.description", "THEAPPRENTICE-REFLECTION+.description",
            "THEAPPRENTICE-UNBURDENING.title", "THEAPPRENTICE-UNBURDENING.description", "THEAPPRENTICE-UNBURDENING+.description",
            "THEAPPRENTICE-PROJECTION.title", "THEAPPRENTICE-PROJECTION.description", "THEAPPRENTICE-PROJECTION+.description",
            "THEAPPRENTICE-DEFIANCE.title", "THEAPPRENTICE-DEFIANCE.description", "THEAPPRENTICE-DEFIANCE+.description",
            "THEAPPRENTICE-TENACITY.title", "THEAPPRENTICE-TENACITY.description", "THEAPPRENTICE-TENACITY+.description",
            "THEAPPRENTICE-FORTITUDE.title", "THEAPPRENTICE-FORTITUDE.description", "THEAPPRENTICE-FORTITUDE+.description",
            "THEAPPRENTICE-UNDERCURRENT.title", "THEAPPRENTICE-UNDERCURRENT.description", "THEAPPRENTICE-UNDERCURRENT+.description",
            "THEAPPRENTICE-CATHARSIS.title", "THEAPPRENTICE-CATHARSIS.description", "THEAPPRENTICE-CATHARSIS+.description",
            "THEAPPRENTICE-TIRADE.title", "THEAPPRENTICE-TIRADE.description", "THEAPPRENTICE-TIRADE+.description",
            "THEAPPRENTICE-SCAPEGOAT.title", "THEAPPRENTICE-SCAPEGOAT.description", "THEAPPRENTICE-SCAPEGOAT+.description", "THEAPPRENTICE-SCAPEGOAT.selectionPrompt",
            "THEAPPRENTICE-RECRIMINATION.title", "THEAPPRENTICE-RECRIMINATION.description", "THEAPPRENTICE-RECRIMINATION+.description",
            "THEAPPRENTICE-TRUE_STRENGTH.title", "THEAPPRENTICE-TRUE_STRENGTH.description", "THEAPPRENTICE-TRUE_STRENGTH+.description",
        ];
        var missing = expectedKeys.Where(k => !dict.ContainsKey(k) || string.IsNullOrWhiteSpace(dict[k])).ToList();
        Assert.True(missing.Count == 0, $"Missing or empty Emotional Expression localization keys:\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void CardsJson_HasAllCrossoverCardKeys()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/cards.json");
        string[] expectedKeys = [
            "THEAPPRENTICE-DESTINED.title", "THEAPPRENTICE-DESTINED.description", "THEAPPRENTICE-DESTINED+.description",
            "THEAPPRENTICE-FLOURISH.title", "THEAPPRENTICE-FLOURISH.description", "THEAPPRENTICE-FLOURISH+.description",
            "THEAPPRENTICE-RESTRAINED_STRIKE.title", "THEAPPRENTICE-RESTRAINED_STRIKE.description", "THEAPPRENTICE-RESTRAINED_STRIKE+.description",
            "THEAPPRENTICE-ACHING_WISH.title", "THEAPPRENTICE-ACHING_WISH.description", "THEAPPRENTICE-ACHING_WISH+.description",
            "THEAPPRENTICE-DESIRE.title", "THEAPPRENTICE-DESIRE.description", "THEAPPRENTICE-DESIRE+.description",
            "THEAPPRENTICE-CATHARTIC_VISION.title", "THEAPPRENTICE-CATHARTIC_VISION.description", "THEAPPRENTICE-CATHARTIC_VISION+.description",
            "THEAPPRENTICE-PROPHECY.title", "THEAPPRENTICE-PROPHECY.description", "THEAPPRENTICE-PROPHECY+.description",
        ];
        var missing = expectedKeys.Where(k => !dict.ContainsKey(k) || string.IsNullOrWhiteSpace(dict[k])).ToList();
        Assert.True(missing.Count == 0, $"Missing or empty crossover localization keys:\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void CardsJson_EmotionalExpressionCards_UseCorrectTextConvention()
    {
        var dict = LoadJson("../TheApprentice/localization/eng/cards.json");
        var eeKeyPrefixes = new[]
        {
            "THEAPPRENTICE-LAMENT", "THEAPPRENTICE-CANTICLE", "THEAPPRENTICE-OUTCRY",
            "THEAPPRENTICE-CONFESSION", "THEAPPRENTICE-REPOSE", "THEAPPRENTICE-CANDOR",
            "THEAPPRENTICE-INVERSION", "THEAPPRENTICE-REVERSAL", "THEAPPRENTICE-DISCORD",
            "THEAPPRENTICE-RELEASE", "THEAPPRENTICE-TRANSFERENCE", "THEAPPRENTICE-REFLECTION",
            "THEAPPRENTICE-UNBURDENING", "THEAPPRENTICE-PROJECTION", "THEAPPRENTICE-DEFIANCE",
            "THEAPPRENTICE-TENACITY", "THEAPPRENTICE-FORTITUDE", "THEAPPRENTICE-UNDERCURRENT",
            "THEAPPRENTICE-CATHARSIS", "THEAPPRENTICE-TIRADE", "THEAPPRENTICE-SCAPEGOAT",
            "THEAPPRENTICE-RECRIMINATION", "THEAPPRENTICE-TRUE_STRENGTH",
        };
        var violations = new List<string>();
        foreach (var prefix in eeKeyPrefixes)
        {
            foreach (var suffix in new[] { ".description", "+.description" })
            {
                var key = prefix + suffix;
                if (!dict.TryGetValue(key, out var text)) continue;
                if (text.Contains("Become [gold]"))
                    violations.Add($"{key}: uses old 'Become [gold]...' convention");
                if (text.Contains("[/gold]("))
                    violations.Add($"{key}: uses old '[/gold](N)' suffix convention");
            }
        }
        Assert.True(violations.Count == 0,
            $"Old text conventions found — update to 'Apply N [gold]X[/gold] to yourself':\n{string.Join("\n", violations)}");
    }

}
