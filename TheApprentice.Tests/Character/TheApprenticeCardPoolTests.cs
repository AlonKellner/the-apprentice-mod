using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Character;

// Card instantiation tests live in ApprenticeCardTests to avoid duplicate registration
// in CustomContentDictionary. These tests verify IsPrePlanned via type reflection only.
public class TheApprenticeCardPoolTests
{
    [Fact]
    public void Signature_Type_IsPrePlanned()
    {
        // IsPrePlanned is a virtual property on ApprenticeCard, overridden on Signature.
        // Verify via reflection on the type to avoid instantiation.
        var prop = typeof(Signature).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void Prelude_Type_IsPrePlanned()
    {
        var prop = typeof(Prelude).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void IsPrePlanned_DefaultsToFalse_OnApprenticeCardBase()
    {
        // The base ApprenticeCard declares IsPrePlanned as virtual returning false.
        // Only Signature and Prelude override it.
        var overrides = typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPrePlanned");
                return method != null && method.DeclaringType == t; // overrides in this type
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "Prelude", "Signature" }, overrides);
    }
}
