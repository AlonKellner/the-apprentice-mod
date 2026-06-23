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
}
