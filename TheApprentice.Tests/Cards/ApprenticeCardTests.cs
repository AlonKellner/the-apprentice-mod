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
    [Fact] public void Improvise_IsSkill_Uncommon_0Cost() { var c = new Improvise(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Preface_IsAttack_Common_0Cost()  { var c = new Preface();   Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Leitmotif_IsAttack_Common_1Cost() { var c = new Leitmotif(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Draft_IsSkill_Common_0Cost()      { var c = new Draft();     Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Foresight_IsSkill_Uncommon_1Cost() { var c = new Foresight(); Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Overture_IsAttack_Uncommon_1Cost() { var c = new Overture();  Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Maestro_IsPower_Rare_2Cost()       { var c = new Maestro();   Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
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

    [Fact] public void Lullaby_IsPower_Uncommon()    { var c = new Lullaby();    Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Conviction_IsPower_Uncommon() { var c = new Conviction(); Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }

    [Fact] public void Reverie_IsSkill_Uncommon()           { var c = new Reverie();          Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Manifesto_IsSkill_Uncommon()          { var c = new Manifesto();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Lucidity_IsSkill_Uncommon()           { var c = new Lucidity();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Resolve_IsSkill_Uncommon()            { var c = new Resolve();          Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Ignition_IsSkill_Uncommon()           { var c = new Ignition();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Sublimation_IsSkill_Uncommon()        { var c = new Sublimation();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Chrysalis_IsSkill_Uncommon()          { var c = new Chrysalis();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Pastiche_IsSkill_Uncommon()           { var c = new Pastiche();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void DrivenInspiration_IsSkill_Uncommon()  { var c = new DrivenInspiration();Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); }
    [Fact] public void Conductor_IsPower_Rare()               { var c = new Conductor();        Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); }

    [Fact] public void Daydream_IsSkill_Rare_Exhaust()   { var c = new Daydream();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Wunderkind_IsPower_Rare() { var c = new Wunderkind();  Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }
    [Fact] public void Prodigy_IsPower_Rare()    { var c = new Prodigy();     Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); }
    [Fact] public void Blueprint_IsSkill_Rare_NoSelfExhaust()  { var c = new Blueprint();   Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.DoesNotContain(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Solace_IsSkill_Common_1Cost()    { var c = new Solace();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Chorus_IsAttack_Common_1Cost()   { var c = new Chorus();    Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Wishful_IsAttack_Common_0Cost()  { var c = new Wishful();   Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Passion_IsAttack_Uncommon_2Cost() { var c = new Passion();  Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Crescendo_IsAttack_Rare_2Cost()   { var c = new Crescendo(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }

    // ── Emotional Expression cards ──────────────────────────────────────────────

    [Fact] public void Lament_IsAttack_Common_0Cost()     { var c = new Lament();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Canticle_IsAttack_Common_1Cost()   { var c = new Canticle();    Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Outcry_IsAttack_Common_1Cost()     { var c = new Outcry();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Confession_IsAttack_Common_1Cost() { var c = new Confession();  Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Repose_IsSkill_Common_1Cost()      { var c = new Repose();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Candor_IsSkill_Common_1Cost()      { var c = new Candor();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Inversion_IsSkill_Common_2Cost_Exhaust() { var c = new Inversion(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Exertion_IsSkill_Common_0Cost()  { var c = new Exertion();  Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Strain_IsSkill_Common_0Cost()    { var c = new Strain();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Wallow_IsSkill_Common_1Cost()    { var c = new Wallow();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Compose_IsAttack_Common_1Cost()  { var c = new Compose();   Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Surrender_IsSkill_Common_1Cost() { var c = new Surrender(); Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }

    [Fact] public void Reversal_IsSkill_Uncommon_1Cost()      { var c = new Reversal();     Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Breakthrough_IsAttack_Uncommon_1Cost() { var c = new Breakthrough(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Dissolution_IsSkill_Uncommon_1Cost()  { var c = new Dissolution();  Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Brace_IsSkill_Uncommon_0Cost()        { var c = new Brace();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Burden_IsAttack_Uncommon_1Cost()       { var c = new Burden();       Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Ensemble_IsSkill_Uncommon_1Cost()      { var c = new Ensemble();     Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Accent_IsSkill_Uncommon_0Cost()        { var c = new Accent();       Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Discord_IsAttack_Uncommon_2Cost()      { var c = new Discord();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Release_IsAttack_Uncommon_1Cost()      { var c = new Release();      Assert.Equal(CardType.Attack,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Transference_IsSkill_Uncommon_1Cost()  { var c = new Transference(); Assert.Equal(CardType.Skill,   c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Reflection_IsSkill_Uncommon_2Cost_Exhaust() { var c = new Reflection(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
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
    [Fact] public void Fermata_IsPower_Rare_2Cost()            { var c = new Fermata();       Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Penance_IsPower_Rare_2Cost()            { var c = new Penance();       Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Fanfare_IsPower_Rare_2Cost_Innate()     { var c = new Fanfare();       Assert.Equal(CardType.Power, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }

    // ── Strength-losing new cards ──────────────────────────────────────────────

    [Fact] public void Diminuendo_IsSkill_Uncommon_1Cost() { var c = new Diminuendo(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Flat_IsSkill_Common_1Cost()         { var c = new Flat();       Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Sforzando_IsSkill_Uncommon_0Cost()  { var c = new Sforzando();  Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }

    // ── Tension set ─────────────────────────────────────────────────────────────

    // Commons
    [Fact] public void Staccato_IsAttack_Common_0Cost() { var c = new Staccato(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); }
    [Fact] public void Brass_IsAttack_Common_1Cost()    { var c = new Brass();    Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Tutti_IsAttack_Uncommon_1Cost()  { var c = new Tutti();    Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Motif_IsSkill_Common_1Cost()     { var c = new Motif();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Strings_IsSkill_Common_1Cost()   { var c = new Strings();  Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Tremolo_IsSkill_Common_1Cost()   { var c = new Tremolo();  Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Score_IsSkill_Uncommon_2Cost()   { var c = new Score();    Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }

    // Uncommons
    [Fact] public void Coda_IsAttack_Common_1Cost()             { var c = new Coda();           Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Reprise_IsAttack_Uncommon_2Cost()         { var c = new Reprise();         Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Vibrato_IsAttack_Uncommon_1Cost()         { var c = new Vibrato();         Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Marcato_IsAttack_Uncommon_1Cost()         { var c = new Marcato();         Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Buildup_IsSkill_Uncommon_1Cost()          { var c = new Buildup();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void DeceptiveCadence_IsSkill_Uncommon_1Cost() { var c = new DeceptiveCadence(); Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Dynamics_IsSkill_Uncommon_0Cost_Retain()  { var c = new Dynamics();        Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Retain); }
    [Fact] public void Refrain_IsSkill_Common_1Cost()            { var c = new Refrain();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Common,   c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Attunement_IsSkill_Uncommon_1Cost()       { var c = new Attunement();      Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Suspension_IsPower_Rare_2Cost()           { var c = new Suspension();      Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Tuning_IsPower_Uncommon_1Cost()           { var c = new Tuning();          Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }

    // Rares
    [Fact] public void Climax_IsAttack_Rare_3Cost()              { var c = new Climax();      Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(3, c.EnergyCost.Canonical); }
    [Fact] public void Triumph_IsSkill_Uncommon_2Cost()          { var c = new Triumph();     Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Tragedy_IsSkill_Rare_0Cost_Exhaust()      { var c = new Tragedy();     Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(0, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Fortissimo_IsPower_Rare_2Cost()           { var c = new Fortissimo();  Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(2, c.EnergyCost.Canonical); }
    [Fact] public void Cadence_IsPower_Rare_3Cost()              { var c = new Cadence();     Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(3, c.EnergyCost.Canonical); }

    // ── Crossover cards ────────────────────────────────────────────────────────

    [Fact] public void Destined_IsSkill_Uncommon_1Cost()         { var c = new Destined();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Flourish_IsAttack_Uncommon_1Cost()        { var c = new Flourish();         Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void RestrainedStrike_IsAttack_Uncommon_1Cost() { var c = new RestrainedStrike(); Assert.Equal(CardType.Attack, c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void AchingWish_IsSkill_Uncommon_1Cost()       { var c = new AchingWish();       Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Uncommon, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Desire_IsPower_Rare_1Cost()               { var c = new Desire();           Assert.Equal(CardType.Power,  c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
    [Fact] public void Transcendence_IsSkill_Rare_1Cost_Exhaust()  { var c = new Transcendence();  Assert.Equal(CardType.Skill, c.Type); Assert.Equal(CardRarity.Rare, c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); Assert.Contains(c.Keywords, k => k == CardKeyword.Exhaust); }
    [Fact] public void Prophecy_IsSkill_Rare_1Cost()             { var c = new Prophecy();         Assert.Equal(CardType.Skill,  c.Type); Assert.Equal(CardRarity.Rare,     c.Rarity); Assert.Equal(1, c.EnergyCost.Canonical); }
}
