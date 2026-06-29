using System.Reflection;
using BaseLib.Patches.Content;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class ApprenticeKeywordsTests
{
    [Fact]
    public void Planned_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact]
    public void Planned_Field_HasCustomEnumAttribute()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }

    [Fact]
    public void Dreamy_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Dreamy", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact]
    public void Dreamy_Field_HasCustomEnumAttribute()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Dreamy", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }

    [Fact]
    public void Ambitous_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Ambitous", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact]
    public void Ambitous_Field_HasCustomEnumAttribute()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Ambitous", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }

    [Fact]
    public void Expend_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Expend", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact]
    public void Expend_Field_HasCustomEnumAttribute()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Expend", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }
}
