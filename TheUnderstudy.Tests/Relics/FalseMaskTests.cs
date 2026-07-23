using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class FalseMaskTests
{
    [Fact]
    public void FalseMask_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new FalseMask().Rarity);

    [Fact]
    public void FalseMask_SelectsExactlyOneCard()
    {
        var prop = typeof(FalseMask).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(1, prop!.GetValue(new FalseMask()));
    }

    [Fact]
    public void FalseMask_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(FalseMask).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // GetUpgradeReplacement()'s body calls ModelDb.Relic<TrueMask>(), which requires the game's
    // model registry (populated only by ModelDb.InitIds() at real game bootstrap) and throws in this
    // bare xUnit harness, so we can only assert the override exists, not its return value. Real
    // behavior (does Touch of Orobas actually swap in True Mask) is /verify-only.
    [Fact]
    public void FalseMask_OverridesGetUpgradeReplacement()
    {
        var method = typeof(FalseMask).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.Equal(typeof(FalseMask), method!.DeclaringType);
    }
}
