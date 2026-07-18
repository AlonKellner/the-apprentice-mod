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
    // The clean "damage/block + one self-debuff" schema cards after the redesign. Meltdown/Dead Weight
    // (also debuff enemies, like Pathos) and the singletons with a non-damage rider (Nervous Energy =
    // +energy, Cram = draw, Burn Out = +energy, Go for Broke = remove-Unplayable) are priced as
    // singletons in SELF_DEBUFF_VALUES.md and not group-constrained here.
    public static IEnumerable<object[]> DamageBlockCards() => new List<object[]>
    {
        // Attack + Block, 2-cost Uncommon (Base 28). Freeze Up moved from 1-cost to 2-cost in the
        // redesign, so it re-stats up into this group (12/12 = 27 vp = 28 + Weak·1's −1).
        new object[] { typeof(FreezeUp),        1, 'W', 28m, 12, 12 },
        new object[] { typeof(HeartAche),       1, 'V', 28m, 10, 16 },
        // Single-hit Attack, 1-cost (Base 15)
        new object[] { typeof(DesperateStrike), 2, 'W', 15m, 20,  0 },
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
        ("attack+block 2-cost Uncommon", new[]
        {
            (typeof(FreezeUp), 1, 'W'), (typeof(HeartAche), 1, 'V'),
        }),
        ("single-hit attack 1-cost", new[]
        {
            (typeof(DesperateStrike), 2, 'W'),
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

}
