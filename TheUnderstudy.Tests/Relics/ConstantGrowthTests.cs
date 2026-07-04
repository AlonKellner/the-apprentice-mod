using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class ConstantGrowthTests
{
    [Fact]
    public void ConstantGrowth_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new ConstantGrowth().Rarity);

    [Fact]
    public void ConstantGrowth_SelectsExactlyTwoCards()
    {
        var prop = typeof(ConstantGrowth).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(2, prop!.GetValue(new ConstantGrowth()));
    }

    [Fact]
    public void ConstantGrowth_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(ConstantGrowth).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // Must stay at the CustomRelicModel default (null) — if this class ever starts declaring its own
    // override that returns itself, a second Touch of Orobas would wrongly "upgrade" Constant Growth
    // into itself instead of correctly falling back to the base game's generic Circlet.
    [Fact]
    public void ConstantGrowth_DoesNotOverrideGetUpgradeReplacement()
    {
        var method = typeof(ConstantGrowth).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.NotEqual(typeof(ConstantGrowth), method!.DeclaringType);
    }
}
