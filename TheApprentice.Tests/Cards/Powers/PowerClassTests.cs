using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Powers;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using TheApprentice.TheApprenticeCode.Character;
using Xunit;

namespace TheApprentice.Tests.Cards.Powers;

public class PowerClassTests
{
    [Fact]
    public void InTheZonePower_IsBuff_Counter()
    {
        var p = new InTheZonePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void ObsessionPower_IsBuff_Single()
    {
        var p = new ObsessionPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void SchemingPower_IsBuff_Single()
    {
        var p = new SchemingPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void SchemingPower_LocalizationTitle_IsScheming()
    {
        var p = new SchemingPower();
        Assert.Contains(p.Localization, e => e.Item1 == "title" && e.Item2 == "Scheming");
    }

    [Fact]
    public void SchemingPower_LocalizationDescriptionAlwaysMentionsChoose()
    {
        var p = new SchemingPower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2).ToList();
        Assert.All(descriptions, d => Assert.Contains("choose", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SchemingPower_OverridesAfterPlayerTurnStartLateNotEarly()
    {
        Assert.NotNull(typeof(SchemingPower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(SchemingPower).GetMethod(
            "AfterPlayerTurnStartEarly", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void SchemingPower_OverridesAfterPlayerTurnStartLate()
    {
        var method = typeof(SchemingPower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }

    [Fact]
    public void SchemingPower_DoesNotOverrideAfterPlayerTurnStart()
    {
        var method = typeof(SchemingPower).GetMethod(
            "AfterPlayerTurnStart", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Null(method);
    }

    [Fact]
    public void VirtuosoPower_IsBuff_Single()
    {
        var p = new VirtuosoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void PlannedCounterPower_IsBuff_Counter()
    {
        var p = new PlannedCounterPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void PlannedCounterPower_HasNoStaticEnsurePresent()
    {
        var method = typeof(PlannedCounterPower)
            .GetMethod("EnsurePresent", BindingFlags.Public | BindingFlags.Static);
        Assert.Null(method);
    }

    [Fact]
    public void ApprenticeCard_OverridesAfterPlayerTurnStartLate()
    {
        var method = typeof(ApprenticeCard).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }

    [Fact]
    public void TheApprenticeCardPool_DoesNotOverrideAfterPlayerTurnStart()
    {
        var method = typeof(TheApprenticeCardPool).GetMethod(
            "AfterPlayerTurnStart", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Null(method);
    }

    [Fact]
    public void UnweakPower_IsBuff_Counter()
    {
        var p = new UnweakPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void UnvulnerablePower_IsBuff_Counter()
    {
        var p = new UnvulnerablePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void TenacityPower_IsBuff_Single()
    {
        var p = new TenacityPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void FortitudePower_IsBuff_Single()
    {
        var p = new FortitudePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void UndercurrentPower_IsBuff_Single()
    {
        var p = new UndercurrentPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void RecriminationPower_IsBuff_Single()
    {
        var p = new RecriminationPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void FermataPower_IsBuff_Single()
    {
        var p = new FermataPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void PenancePower_IsBuff_Counter()
    {
        var p = new PenancePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void FanfarePower_IsBuff_Single()
    {
        var p = new FanfarePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void TenacityPower_Localization_DescriptionMentionsVigor()
    {
        var p = new TenacityPower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2).ToList();
        Assert.All(descriptions, d => Assert.Contains("Vigor", d, StringComparison.Ordinal));
    }

    [Fact]
    public void TenacityPower_Localization_DescriptionDoesNotMentionStrength()
    {
        var p = new TenacityPower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2).ToList();
        Assert.All(descriptions, d => Assert.DoesNotContain("Strength", d, StringComparison.Ordinal));
    }

    [Fact]
    public void FortitudePower_Localization_DescriptionMentionsVigor()
    {
        var p = new FortitudePower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2).ToList();
        Assert.All(descriptions, d => Assert.Contains("Vigor", d, StringComparison.Ordinal));
    }

    [Fact]
    public void FortitudePower_Localization_DescriptionDoesNotMentionStrength()
    {
        var p = new FortitudePower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2).ToList();
        Assert.All(descriptions, d => Assert.DoesNotContain("Strength", d, StringComparison.Ordinal));
    }

    // ── Tension infrastructure powers ─────────────────────────────────────────

    [Fact]
    public void TensionPower_IsDebuff_Counter()
    {
        var p = new TensionPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void DeceptiveCadencePower_IsBuff_Single()
    {
        var p = new DeceptiveCadencePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void DynamicsNextCardPower_IsBuff_Counter()
    {
        var p = new DynamicsNextCardPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void TuningPower_IsBuff_Counter()
    {
        var p = new TuningPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void CadencePower_IsBuff_Counter()
    {
        var p = new CadencePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void FortissimoPower_IsBuff_Counter()
    {
        var p = new FortissimoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void SuspensionPower_IsBuff_Single()
    {
        var p = new SuspensionPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void AllPowers_Localization_IsNonEmpty()
    {
        Assert.NotEmpty(new InTheZonePower().Localization);
        Assert.NotEmpty(new ObsessionPower().Localization);
        Assert.NotEmpty(new SchemingPower().Localization);
        Assert.NotEmpty(new VirtuosoPower().Localization);
        Assert.NotEmpty(new PlannedCounterPower().Localization);
        Assert.NotEmpty(new UnweakPower().Localization);
        Assert.NotEmpty(new UnvulnerablePower().Localization);
        Assert.NotEmpty(new TenacityPower().Localization);
        Assert.NotEmpty(new FortitudePower().Localization);
        Assert.NotEmpty(new UndercurrentPower().Localization);
        Assert.NotEmpty(new RecriminationPower().Localization);
        Assert.NotEmpty(new FermataPower().Localization);
        Assert.NotEmpty(new PenancePower().Localization);
        Assert.NotEmpty(new FanfarePower().Localization);
        Assert.NotEmpty(new AccentPower().Localization);
        Assert.NotEmpty(new TensionPower().Localization);
        Assert.NotEmpty(new DeceptiveCadencePower().Localization);
        Assert.NotEmpty(new DynamicsNextCardPower().Localization);
        Assert.NotEmpty(new TuningPower().Localization);
        Assert.NotEmpty(new CadencePower().Localization);
        Assert.NotEmpty(new FortissimoPower().Localization);
        Assert.NotEmpty(new SuspensionPower().Localization);
    }

    [Fact]
    public void PlannedCounterPower_HasUpdateDisplayIfChanged()
    {
        var method = typeof(PlannedCounterPower)
            .GetMethod("UpdateDisplayIfChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }
}
