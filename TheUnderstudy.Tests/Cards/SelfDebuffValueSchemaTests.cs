using System;
using System.Collections.Generic;
using System.Linq;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Enforces the self-debuff value schema documented in SELF_DEBUFF_VALUES.md:
//   vp(card) = Base(mechanic, cost, rarity) + P*(stacks-1) + sev(debuff)
//   1 damage = 1 vp, 1 block = 1.25 vp  (block is worth ~25% more)
// Because P (6) exceeds the full severity spread (4), every (stacks, debuff) pair gets a unique,
// totally-ordered offset — stack count dominates, debuff type breaks ties, no same-mechanic duplicates.
public class SelfDebuffValueSchemaTests
{
    private const int P = 6;               // vp per extra self-debuff stack (the dominant term)
    private const decimal BlockVp = 1.25m; // block valued 25% above damage

    // Severity offset, worst -> best: Vulnerable > Jaded > Shaken > Weak > Limited (1 vp per step).
    private static int Sev(char debuff) => debuff switch
    {
        'V' => 2, 'J' => 1, 'S' => 0, 'W' => -1, 'L' => -2,
        _ => throw new ArgumentException($"unknown debuff {debuff}"),
    };

    private static int Offset(int stacks, char debuff) => P * (stacks - 1) + Sev(debuff);

    private static UnderstudyCard New(Type t) => (UnderstudyCard)Activator.CreateInstance(t)!;

    private static (int dmg, int block) Stats(UnderstudyCard c)
    {
        int dmg = c.DynamicVars.ContainsKey("Damage") ? (int)c.DynamicVars["Damage"].BaseValue : 0;
        int block = c.DynamicVars.ContainsKey("Block") ? (int)c.DynamicVars["Block"].BaseValue : 0;
        return (dmg, block);
    }

    private static decimal Vp(int dmg, int block) => dmg + BlockVp * block;

    // Damage/Block cards: (type, stacks, debuff, groupBase, expectedDmg, expectedBlock).
    public static IEnumerable<object[]> DamageBlockCards() => new List<object[]>
    {
        // Attack + Block, 1-cost Common (Base 16)
        new object[] { typeof(FreezeUp),        1, 'W', 16m,  5,  8 },
        new object[] { typeof(Matinee),         1, 'J', 16m,  7,  8 },
        new object[] { typeof(Butterflies),     2, 'S', 16m,  7, 12 },
        // Attack + Block, 2-cost Uncommon (Base 23.5)
        new object[] { typeof(Overcommit),      1, 'V', 23.5m, 8, 14 },
        new object[] { typeof(Overexert),       2, 'L', 23.5m, 10, 14 },
        // Single-hit Attack, 1-cost (Base 13)
        new object[] { typeof(Downstage),       1, 'V', 13m, 15,  0 },
        new object[] { typeof(QuickNap),        1, 'J', 13m, 14,  0 },
        new object[] { typeof(StageWhisper),    2, 'W', 13m, 18,  0 },
        // AoE Attack, 1-cost Uncommon (Base 4)
        new object[] { typeof(WideOpen),        1, 'V', 4m,   6,  0 },
        new object[] { typeof(TakeCenterStage), 2, 'S', 4m,  10,  0 },
    };

    [Theory]
    [MemberData(nameof(DamageBlockCards))]
    public void Card_MatchesSchemaStatline(Type type, int stacks, char debuff, decimal groupBase, int expDmg, int expBlock)
    {
        var (dmg, block) = Stats(New(type));
        // (1) the card carries exactly the intended statline
        Assert.Equal((expDmg, expBlock), (dmg, block));
        // (2) that statline's value follows the schema formula for this card's (stacks, debuff)
        Assert.Equal(groupBase + Offset(stacks, debuff), Vp(dmg, block));
    }

    // Same-mechanic groups: within each, every card must have a distinct statline and vp must rise with
    // the schema offset (2-stack always beats 1-stack; harsher debuff breaks ties).
    private static readonly (string name, (Type type, int stacks, char debuff)[] members)[] Groups =
    {
        ("attack+block 1-cost Common", new[]
        {
            (typeof(FreezeUp), 1, 'W'), (typeof(Matinee), 1, 'J'), (typeof(Butterflies), 2, 'S'),
        }),
        ("attack+block 2-cost Uncommon", new[]
        {
            (typeof(Overcommit), 1, 'V'), (typeof(Overexert), 2, 'L'),
        }),
        ("single-hit attack 1-cost", new[]
        {
            (typeof(Downstage), 1, 'V'), (typeof(QuickNap), 1, 'J'), (typeof(StageWhisper), 2, 'W'),
        }),
        ("AoE attack 1-cost Uncommon", new[]
        {
            (typeof(WideOpen), 1, 'V'), (typeof(TakeCenterStage), 2, 'S'),
        }),
    };

    [Fact]
    public void EachMechanicGroup_HasDistinctStatlines()
    {
        foreach (var (name, members) in Groups)
        {
            var lines = members.Select(m => Stats(New(m.type))).ToList();
            Assert.True(lines.Distinct().Count() == lines.Count,
                $"{name}: duplicate statline among {string.Join(", ", lines)}");
        }
    }

    [Fact]
    public void EachMechanicGroup_VpRisesWithSchemaOffset()
    {
        foreach (var (name, members) in Groups)
        {
            var ordered = members
                .Select(m => (offset: Offset(m.stacks, m.debuff), vp: Vp(Stats(New(m.type)).dmg, Stats(New(m.type)).block)))
                .OrderBy(x => x.offset)
                .ToList();
            for (int i = 1; i < ordered.Count; i++)
                Assert.True(ordered[i].vp > ordered[i - 1].vp,
                    $"{name}: vp not strictly increasing with offset ({ordered[i - 1].vp} -> {ordered[i].vp})");
        }
    }

    [Fact]
    public void AllNighter_GainsTwoEnergy()
    {
        var card = New(typeof(AllNighter));
        Assert.Equal(2, (int)card.DynamicVars["Energy"].BaseValue);
    }
}
