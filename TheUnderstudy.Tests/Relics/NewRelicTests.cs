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

// Shape tests for the reward-pool relics (1 Common / 2 Uncommon / 3 Rare / 1 Shop). Their hook bodies need
// a live combat and are verified in-game; here we assert the bare-instantiable surface (rarity, pool
// wiring, non-empty inline loc), same convention as TrueMaskTests.
public class NewRelicTests
{
    public static IEnumerable<object[]> Relics => new List<object[]>
    {
        new object[] { typeof(GoldenCape), RelicRarity.Common },
        new object[] { typeof(Lampshade), RelicRarity.Uncommon },
        new object[] { typeof(Greasepaint), RelicRarity.Uncommon },
        new object[] { typeof(FoldableStage), RelicRarity.Rare },
        new object[] { typeof(DraftingPaper), RelicRarity.Rare },
        new object[] { typeof(Rosin), RelicRarity.Rare },
        new object[] { typeof(Lozenge), RelicRarity.Shop },
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
        var loc = LocText.Of(type);
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
