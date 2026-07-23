using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class TrueMaskTests
{
    [Fact]
    public void TrueMask_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new TrueMask().Rarity);

    [Fact]
    public void TrueMask_SelectsExactlyTwoCards()
    {
        var prop = typeof(TrueMask).GetProperty("SelectCount", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        Assert.Equal(2, prop!.GetValue(new TrueMask()));
    }

    [Fact]
    public void TrueMask_IsMarkedWithRelicPoolAttribute()
    {
        var attr = typeof(TrueMask).GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    // Must stay at the CustomRelicModel default (null) — if this class ever starts declaring its own
    // override that returns itself, a second Touch of Orobas would wrongly "upgrade" True Mask
    // into itself instead of correctly falling back to the base game's generic Circlet.
    [Fact]
    public void TrueMask_DoesNotOverrideGetUpgradeReplacement()
    {
        var method = typeof(TrueMask).GetMethod("GetUpgradeReplacement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.NotEqual(typeof(TrueMask), method!.DeclaringType);
    }
}
