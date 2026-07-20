using System;
using System.Collections.Generic;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

// Shape tests for the five non-rest-site relics (Rosin, Lozenge, Safety Net, Cue Light, Greasepaint).
// Their hook bodies need a live combat and are verified in-game; here we assert the bare-instantiable
// surface (rarity, pool wiring, non-empty inline loc), same convention as ConstantGrowthTests.
public class NewRelicTests
{
    public static IEnumerable<object[]> Relics => new List<object[]>
    {
        new object[] { typeof(Rosin), RelicRarity.Uncommon },
        new object[] { typeof(Lozenge), RelicRarity.Uncommon },
        new object[] { typeof(SafetyNet), RelicRarity.Common },
        new object[] { typeof(CueLight), RelicRarity.Uncommon },
        new object[] { typeof(Greasepaint), RelicRarity.Uncommon },
    };

    private static CustomRelicModel Create(Type type) => (CustomRelicModel)Activator.CreateInstance(type)!;

    [Theory]
    [MemberData(nameof(Relics))]
    public void Relic_HasExpectedRarity(Type type, RelicRarity expected) =>
        Assert.Equal(expected, Create(type).Rarity);

    [Theory]
    [MemberData(nameof(Relics))]
    public void Relic_IsMarkedWithRelicPoolAttribute(Type type, RelicRarity _)
    {
        var attr = type.GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyRelicPool), attr!.PoolType);
    }

    [Theory]
    [MemberData(nameof(Relics))]
    public void Relic_HasNonEmptyLocalization(Type type, RelicRarity _)
    {
        var loc = Create(type).Localization;
        Assert.NotNull(loc);
        Assert.NotEmpty(loc!);
    }
}

// The only real bare-testable logic in the counter relics: the pure threshold fold.
public class UnderstudyCounterRelicTests
{
    [Theory]
    [InlineData(0, 1, 5, 0, 1)]   // first tick
    [InlineData(4, 1, 5, 1, 0)]   // 5th tick fires and rolls over to 0
    [InlineData(3, 1, 5, 0, 4)]   // mid progress
    [InlineData(0, 5, 5, 1, 0)]   // a batch of 5 at once fires once
    [InlineData(4, 3, 5, 1, 2)]   // crosses once, leftover carries
    public void Advance_FoldsAtThreshold(int counter, int add, int threshold, int expFires, int expRemainder)
    {
        var (fires, remainder) = UnderstudyCounterRelic.Advance(counter, add, threshold);
        Assert.Equal(expFires, fires);
        Assert.Equal(expRemainder, remainder);
    }
}
