using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class PowerClassTests
{
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
    public void LimitedPower_IsDebuff_Counter()
    {
        var p = new LimitedPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void UnlimitedPower_IsBuff_Counter()
    {
        var p = new UnlimitedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void LimitedPower_Localization_MentionsDrawFewer()
    {
        var p = new LimitedPower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("fewer", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnlimitedPower_Localization_MentionsHandFull()
    {
        var p = new UnlimitedPower();
        var descriptions = p.Localization.Where(e => e.Item1 == "description").Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("full", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LimitedPower_OverridesModifyHandDraw()
    {
        Assert.NotNull(typeof(LimitedPower).GetMethod(
            "ModifyHandDraw", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void UnlimitedPower_OverridesModifyHandDraw()
    {
        Assert.NotNull(typeof(UnlimitedPower).GetMethod(
            "ModifyHandDraw", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void AllPowers_Localization_IsNonEmpty()
    {
        Assert.NotEmpty(new PlannedCounterPower().Localization);
        Assert.NotEmpty(new UnweakPower().Localization);
        Assert.NotEmpty(new UnvulnerablePower().Localization);
        Assert.NotEmpty(new LimitedPower().Localization);
        Assert.NotEmpty(new UnlimitedPower().Localization);
    }

    [Fact]
    public void PlannedCounterPower_HasUpdateDisplayIfChanged()
    {
        var method = typeof(PlannedCounterPower)
            .GetMethod("UpdateDisplayIfChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }
}
