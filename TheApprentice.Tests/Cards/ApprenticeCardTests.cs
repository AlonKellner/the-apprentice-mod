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
    public void NewPerspective_InheritsFromApprenticeCard() =>
        Assert.True(typeof(NewPerspective).IsSubclassOf(typeof(ApprenticeCard)));

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
    [Fact] public void ClearMind_IsAttack_Rare_NotPrePlanned() { var c = new ClearMind(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void NewPerspective_IsSkill_Common_NotPrePlanned() { var c = new NewPerspective(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void TabulaRasa_IsSkill_Uncommon_NotPrePlanned() { var c = new TabulaRasa(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Transpose_IsSkill_Uncommon_NotPrePlanned() { var c = new Transpose(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Signature_IsAttack_Uncommon_IsPrePlanned() { var c = new Signature(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.True(c.IsPrePlanned); }
    [Fact] public void Prelude_IsSkill_Uncommon_IsPrePlanned() { var c = new Prelude(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.True(c.IsPrePlanned); }
    [Fact] public void Scheming_IsPower_Uncommon_NotPrePlanned() { var c = new Scheming(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void InTheZone_IsPower_Rare_NotPrePlanned() { var c = new InTheZone(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Obsession_IsPower_Rare_NotPrePlanned() { var c = new Obsession(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Encore_IsSkill_Rare_NotPrePlanned() { var c = new Encore(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Virtuoso_IsPower_Rare_NotPrePlanned() { var c = new Virtuoso(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void MagnumOpus_IsSkill_Rare_NotPrePlanned() { var c = new MagnumOpus(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.False(c.IsPrePlanned); }

    // ── Ephemeral cards (not in card library) ──────────────────────────────────

    [Fact]
    public void Dream_IsSkill_Token_NotInCardLibrary()
    {
        var c = new Dream();
        Assert.Equal(CardType.Skill, c.Type);
        Assert.Equal(CardRarity.Token, c.Rarity);
        Assert.False(c.ShouldShowInCardLibrary);
        Assert.Equal(0, c.EnergyCost.Canonical);
    }

    [Fact]
    public void Ambition_IsAttack_Token_NotInCardLibrary()
    {
        var c = new Ambition();
        Assert.Equal(CardType.Attack, c.Type);
        Assert.Equal(CardRarity.Token, c.Rarity);
        Assert.False(c.ShouldShowInCardLibrary);
        Assert.Equal(0, c.EnergyCost.Canonical);
    }

    [Fact]
    public void Potential_IsAttack_Token_NotInCardLibrary()
    {
        var c = new Potential();
        Assert.Equal(CardType.Attack, c.Type);
        Assert.Equal(CardRarity.Token, c.Rarity);
        Assert.False(c.ShouldShowInCardLibrary);
        Assert.Equal(0, c.EnergyCost.Canonical);
    }

    // ── Dreams & Ambitions obtainable cards ────────────────────────────────────

    [Fact] public void Nocturne_IsSkill_Common()    { var c = new Nocturne();   Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Hubris_IsAttack_Common()     { var c = new Hubris();     Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Longing_IsSkill_Common()     { var c = new Longing();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.False(c.IsPrePlanned); }
    [Fact] public void Drive_IsAttack_Common()      { var c = new Drive();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.False(c.IsPrePlanned); }

    [Fact] public void Lullaby_IsPower_Common()    { var c = new Lullaby();    Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); }
    [Fact] public void Conviction_IsPower_Common() { var c = new Conviction(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); }

    [Fact] public void Reverie_IsSkill_Uncommon()           { var c = new Reverie();          Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Manifesto_IsSkill_Uncommon()          { var c = new Manifesto();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Lucidity_IsSkill_Uncommon()           { var c = new Lucidity();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Resolve_IsSkill_Uncommon()            { var c = new Resolve();          Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Ignition_IsSkill_Uncommon()           { var c = new Ignition();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Sublimation_IsSkill_Uncommon()        { var c = new Sublimation();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Chrysalis_IsSkill_Uncommon()          { var c = new Chrysalis();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Pastiche_IsSkill_Uncommon()           { var c = new Pastiche();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void DrivenInspiration_IsSkill_Uncommon()  { var c = new DrivenInspiration();Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Conductor_IsPower_Uncommon()          { var c = new Conductor();        Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }

    [Fact] public void Daydream_IsSkill_Rare()   { var c = new Daydream();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }
    [Fact] public void Wunderkind_IsPower_Rare() { var c = new Wunderkind();  Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }
    [Fact] public void Prodigy_IsPower_Rare()    { var c = new Prodigy();     Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }
    [Fact] public void Blueprint_IsSkill_Rare()  { var c = new Blueprint();   Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }

    // ── Emotional Expression cards ──────────────────────────────────────────────

    [Fact] public void Lament_IsAttack_Common_0Cost()     { var c = new Lament();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Canticle_IsAttack_Common_1Cost()   { var c = new Canticle();    Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Outcry_IsAttack_Common_1Cost()     { var c = new Outcry();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Confession_IsAttack_Common_1Cost() { var c = new Confession();  Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Repose_IsSkill_Common_1Cost()      { var c = new Repose();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Candor_IsSkill_Common_1Cost()      { var c = new Candor();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Inversion_IsSkill_Common_2Cost_Exhaust() { var c = new Inversion(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }

    [Fact] public void Reversal_IsAttack_Uncommon_1Cost()     { var c = new Reversal();     Assert.Equal(CardType.Attack,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Discord_IsAttack_Uncommon_2Cost()      { var c = new Discord();      Assert.Equal(CardType.Attack,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Release_IsAttack_Uncommon_1Cost()      { var c = new Release();      Assert.Equal(CardType.Attack,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Transference_IsSkill_Uncommon_1Cost()  { var c = new Transference(); Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Reflection_IsSkill_Uncommon_1Cost()    { var c = new Reflection();   Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Unburdening_IsSkill_Uncommon_0Cost()   { var c = new Unburdening();  Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Projection_IsSkill_Uncommon_1Cost()    { var c = new Projection();   Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Defiance_IsSkill_Uncommon_1Cost()      { var c = new Defiance();     Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Tenacity_IsPower_Uncommon_1Cost()      { var c = new Tenacity();     Assert.Equal(CardType.Power,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Fortitude_IsPower_Uncommon_1Cost()     { var c = new Fortitude();    Assert.Equal(CardType.Power,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Undercurrent_IsPower_Uncommon_2Cost()  { var c = new Undercurrent(); Assert.Equal(CardType.Power,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }

    [Fact] public void Catharsis_IsAttack_Rare_0Cost_Exhaust() { var c = new Catharsis(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Tirade_IsAttack_Rare_2Cost()            { var c = new Tirade();     Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Scapegoat_IsSkill_Rare_1Cost()          { var c = new Scapegoat();  Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Recrimination_IsPower_Rare_1Cost()      { var c = new Recrimination(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void TrueStrength_IsPower_Rare_2Cost()       { var c = new TrueStrength();  Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
}
