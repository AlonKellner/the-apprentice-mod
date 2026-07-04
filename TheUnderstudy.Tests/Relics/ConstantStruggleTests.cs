using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class ConstantStruggleTests
{
    [Fact]
    public void ConstantStruggle_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new ConstantStruggle().Rarity);

    [Fact]
    public void ConstantStruggle_SelectsExactlyOneCard()
    {
        var prop = typeof(ConstantStruggle).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(1, prop!.GetValue(new ConstantStruggle()));
    }

    [Fact]
    public void ConstantStruggle_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(ConstantStruggle).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // GetUpgradeReplacement()'s body calls ModelDb.Relic<ConstantGrowth>(), which requires the game's
    // model registry (populated only by ModelDb.InitIds() at real game bootstrap) and throws in this
    // bare xUnit harness, so we can only assert the override exists, not its return value. Real
    // behavior (does Touch of Orobas actually swap in Constant Growth) is /verify-only.
    [Fact]
    public void ConstantStruggle_OverridesGetUpgradeReplacement()
    {
        var method = typeof(ConstantStruggle).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.Equal(typeof(ConstantStruggle), method!.DeclaringType);
    }
}
