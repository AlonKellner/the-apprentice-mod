using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Character;
using Xunit;

namespace TheApprentice.Tests.Cards;

// Each card type may only be instantiated once across all tests (CustomContentDictionary
// registers each class on first instantiation and rejects duplicates). Each card gets
// exactly one test method here covering all its static properties.
public class ApprenticeCardTests
{
    // ── Type hierarchy (no instantiation — safe to run alongside anything) ────

    [Fact]
    public void ApprenticeStrike_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ApprenticeStrike).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact]
    public void ApprenticeDefend_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ApprenticeDefend).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact]
    public void Plan_InheritsFromApprenticeCard() =>
        Assert.True(typeof(Plan).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact]
    public void JustAsPlanned_InheritsFromApprenticeCard() =>
        Assert.True(typeof(JustAsPlanned).IsSubclassOf(typeof(ApprenticeCard)));

    [Fact]
    public void ScrapPlans_InheritsFromApprenticeCard() =>
        Assert.True(typeof(ScrapPlans).IsSubclassOf(typeof(ApprenticeCard)));

    // ── Pool attribute ─────────────────────────────────────────────

    [Fact]
    public void ApprenticeCard_HasPoolAttribute()
    {
        var attr = typeof(ApprenticeCard)
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.Name == nameof(PoolAttribute));
        Assert.NotNull(attr);
    }

    [Fact]
    public void ApprenticeCard_PoolAttribute_TargetsApprenticeCardPool()
    {
        var attr = typeof(ApprenticeCard)
            .GetCustomAttributesData()
            .First(a => a.AttributeType.Name == nameof(PoolAttribute));
        var poolType = attr.ConstructorArguments[0].Value as Type;
        Assert.Equal(typeof(TheApprenticeCardPool), poolType);
    }

    [Fact]
    public void ApprenticeCard_PoolAttribute_IsOnBaseClass()
    {
        // PoolAttribute lives on ApprenticeCard itself; subclasses inherit behavior at runtime.
        var attr = typeof(ApprenticeCard)
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.Name == nameof(PoolAttribute));
        Assert.NotNull(attr);
    }

    // ── New card: type / rarity / IsPrePlanned (one instantiation per card) ──

    [Fact] public void Contemplate_IsSkill_Common_NotPrePlanned() { var c = new Contemplate(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Realize_IsAttack_Common_NotPrePlanned() { var c = new Realize(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Epiphany_IsSkill_Common_NotPrePlanned() { var c = new Epiphany(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Groove_IsAttack_Uncommon_NotPrePlanned() { var c = new Groove(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Overthinking_IsSkill_Uncommon_NotPrePlanned() { var c = new Overthinking(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void CreativeBlock_IsSkill_Uncommon_NotPrePlanned() { var c = new CreativeBlock(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Rehearsal_IsSkill_Uncommon_NotPrePlanned() { var c = new Rehearsal(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void ClearMind_IsSkill_Rare_NotPrePlanned() { var c = new ClearMind(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void TabulaRasa_IsSkill_Uncommon_NotPrePlanned() { var c = new TabulaRasa(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Transpose_IsSkill_Uncommon_NotPrePlanned() { var c = new Transpose(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Signature_IsAttack_Uncommon_IsPrePlanned() { var c = new Signature(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.True(c.IsPrePlanned); }
    [Fact] public void Prelude_IsSkill_Uncommon_IsPrePlanned() { var c = new Prelude(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.True(c.IsPrePlanned); }
    [Fact] public void MethodToTheMadness_IsPower_Uncommon_NotPrePlanned() { var c = new MethodToTheMadness(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void InTheZone_IsPower_Rare_NotPrePlanned() { var c = new InTheZone(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Obsession_IsPower_Rare_NotPrePlanned() { var c = new Obsession(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Encore_IsSkill_Rare_NotPrePlanned() { var c = new Encore(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Virtuoso_IsPower_Rare_NotPrePlanned() { var c = new Virtuoso(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void MagnumOpus_IsSkill_Rare_NotPrePlanned() { var c = new MagnumOpus(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
}
