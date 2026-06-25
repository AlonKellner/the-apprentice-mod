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
    public void ObsessionPower_IsBuff_Counter()
    {
        var p = new ObsessionPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
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
    public void AllPowers_Localization_IsNonEmpty()
    {
        Assert.NotEmpty(new InTheZonePower().Localization);
        Assert.NotEmpty(new ObsessionPower().Localization);
        Assert.NotEmpty(new SchemingPower().Localization);
        Assert.NotEmpty(new VirtuosoPower().Localization);
        Assert.NotEmpty(new PlannedCounterPower().Localization);
    }

    [Fact]
    public void PlannedCounterPower_HasUpdateDisplayIfChanged()
    {
        var method = typeof(PlannedCounterPower)
            .GetMethod("UpdateDisplayIfChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }
}
