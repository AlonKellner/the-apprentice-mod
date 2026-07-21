using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class HardEarnedFeatTests
{
    [Fact]
    public void HardEarnedFeat_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new HardEarnedFeat().Rarity);

    [Fact]
    public void HardEarnedFeat_SelectsExactlyTwoCards()
    {
        var prop = typeof(HardEarnedFeat).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(2, prop!.GetValue(new HardEarnedFeat()));
    }

    [Fact]
    public void HardEarnedFeat_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(HardEarnedFeat).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // Must stay at the CustomRelicModel default (null) — if this class ever starts declaring its own
    // override that returns itself, a second Touch of Orobas would wrongly "upgrade" Hard Earned Feat
    // into itself instead of correctly falling back to the base game's generic Circlet.
    [Fact]
    public void HardEarnedFeat_DoesNotOverrideGetUpgradeReplacement()
    {
        var method = typeof(HardEarnedFeat).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.NotEqual(typeof(HardEarnedFeat), method!.DeclaringType);
    }
}
