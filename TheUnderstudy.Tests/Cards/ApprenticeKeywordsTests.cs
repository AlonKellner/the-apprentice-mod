using System.Reflection;
using BaseLib.Patches.Content;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

public class UnderstudyKeywordsTests
{
    [Fact]
    public void Planned_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(UnderstudyKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact]
    public void Planned_Field_HasCustomEnumAttribute()
    {
        var field = typeof(UnderstudyKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }
}
