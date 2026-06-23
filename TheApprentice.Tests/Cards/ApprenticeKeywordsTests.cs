using System.Reflection;
using BaseLib.Patches.Content;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

// Tests reference ApprenticeKeywords.Planned which requires CardKeyword (sts2.dll, x86_64).
// On ARM64 they are skipped; they serve as documentation and are verified on x86_64.
public class ApprenticeKeywordsTests
{
    private const string SkipReason = "Requires sts2.dll (x86_64) which cannot load in arm64 test runner";

    [Fact(Skip = SkipReason)]
    public void Planned_Field_Exists_And_IsPublicStatic()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
    }

    [Fact(Skip = SkipReason)]
    public void Planned_Field_HasCustomEnumAttribute()
    {
        var field = typeof(ApprenticeKeywords)
            .GetField("Planned", BindingFlags.Public | BindingFlags.Static)!;
        Assert.NotNull(field.GetCustomAttribute<CustomEnumAttribute>());
    }
}
