using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Potions;
using Xunit;

namespace TheUnderstudy.Tests.Potions;

// Shape tests for the 3 potions (one per rarity). OnUse bodies need a live ModelDb/combat and are verified
// in-game; here we assert the pool wiring, usage/target config, and each potion's rarity (the reflected,
// bare-instantiable surface — same convention as the relic tests).
public class PotionTests
{
    public static IEnumerable<object[]> AllPotions => new List<object[]>
    {
        new object[] { typeof(VigorPotion), PotionRarity.Common },
        new object[] { typeof(Milkshake), PotionRarity.Uncommon },
        new object[] { typeof(GlitterBomb), PotionRarity.Rare },
    };

    private static UnderstudyPotion Create(Type type) => (UnderstudyPotion)Activator.CreateInstance(type)!;

    [Theory]
    [MemberData(nameof(AllPotions))]
    public void Potion_HasExpectedRarity(Type type, PotionRarity expected) =>
        Assert.Equal(expected, Create(type).Rarity);

    [Theory]
    [MemberData(nameof(AllPotions))]
    public void Potion_IsCombatOnlyAndUntargeted(Type type, PotionRarity _)
    {
        var potion = Create(type);
        Assert.Equal(PotionUsage.CombatOnly, potion.Usage);
        Assert.Equal(TargetType.None, potion.TargetType);
    }

    [Theory]
    [MemberData(nameof(AllPotions))]
    public void Potion_IsMarkedWithPotionPoolAttribute(Type type, PotionRarity _)
    {
        var attr = type.GetCustomAttribute<PoolAttribute>(inherit: true);
        Assert.NotNull(attr);
        Assert.Equal(typeof(TheUnderstudyPotionPool), attr!.PoolType);
    }

    // Guards against adding a potion class but forgetting to cover it above (so it silently ships
    // without a rarity/pool assertion).
    [Fact]
    public void EveryConcretePotion_IsCovered()
    {
        var covered = AllPotions.Select(d => (Type)d[0]).OrderBy(t => t.Name).ToArray();
        var discovered = typeof(UnderstudyPotion).Assembly.GetTypes()
            .Where(t => typeof(UnderstudyPotion).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();
        Assert.Equal(discovered, covered);
    }
}
