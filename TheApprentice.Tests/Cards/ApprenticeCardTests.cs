using BaseLib.Utils;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Character;
using Xunit;

namespace TheApprentice.Tests.Cards;

// All tests here reference ApprenticeCard which inherits from ConstructedCardModel (BaseLib/sts2).
// sts2.dll is x86_64 and cannot be loaded by the arm64 test runner, so they are skipped here.
// They verify the correct structure when run on an x86_64 machine.
public class ApprenticeCardTests
{
    private const string SkipReason = "Requires sts2.dll (x86_64) which cannot load in arm64 test runner";

    // ── Type hierarchy ─────────────────────────────────────────────

    [Fact(Skip = SkipReason)]
    public void ApprenticeStrike_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ApprenticeStrike).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact(Skip = SkipReason)]
    public void ApprenticeDefend_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ApprenticeDefend).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact(Skip = SkipReason)]
    public void Plan_InheritsFromApprenticeCard() =>
        Assert.True(typeof(Plan).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact(Skip = SkipReason)]
    public void ExecutePlans_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ExecutePlans).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact(Skip = SkipReason)]
    public void ScrapPlans_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ScrapPlans).IsSubclassOf(typeof(ApprenticeCard)));

    // ── Pool attribute ─────────────────────────────────────────────

    [Fact(Skip = SkipReason)]
    public void ApprenticeCard_HasPoolAttribute()
    {
        var attr = typeof(ApprenticeCard)
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.Name == nameof(PoolAttribute));
        Assert.NotNull(attr);
    }

    [Fact(Skip = SkipReason)]
    public void ApprenticeCard_PoolAttribute_TargetsApprenticeCardPool()
    {
        var attr = typeof(ApprenticeCard)
            .GetCustomAttributesData()
            .First(a => a.AttributeType.Name == nameof(PoolAttribute));
        var poolType = attr.ConstructorArguments[0].Value as Type;
        Assert.Equal(typeof(TheApprenticeCardPool), poolType);
    }

    [Fact(Skip = SkipReason)]
    public void ApprenticeCard_PoolAttribute_IsInherited()
    {
        // AttributeUsage on PoolAttribute has Inherited = true, so subclasses carry it.
        var attr = typeof(ApprenticeStrike)
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.Name == nameof(PoolAttribute));
        Assert.NotNull(attr);
    }
}
