using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class ShamefulGiftTests
{
    [Fact]
    public void ShamefulGift_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new ShamefulGift().Rarity);

    [Fact]
    public void ShamefulGift_SelectsExactlyOneCard()
    {
        var prop = typeof(ShamefulGift).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(1, prop!.GetValue(new ShamefulGift()));
    }

    [Fact]
    public void ShamefulGift_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(ShamefulGift).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // GetUpgradeReplacement()'s body calls ModelDb.Relic<HardEarnedFeat>(), which requires the game's
    // model registry (populated only by ModelDb.InitIds() at real game bootstrap) and throws in this
    // bare xUnit harness, so we can only assert the override exists, not its return value. Real
    // behavior (does Touch of Orobas actually swap in Hard Earned Feat) is /verify-only.
    [Fact]
    public void ShamefulGift_OverridesGetUpgradeReplacement()
    {
        var method = typeof(ShamefulGift).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.Equal(typeof(ShamefulGift), method!.DeclaringType);
    }
}
