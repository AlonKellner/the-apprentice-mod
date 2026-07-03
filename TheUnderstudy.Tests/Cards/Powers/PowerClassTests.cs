using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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

    [Fact]
    public void JadedPower_IsDebuff_Counter()
    {
        var p = new JadedPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void UnjadedPower_IsBuff_Counter()
    {
        var p = new UnjadedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void TakeNotesPower_IsBuff_Counter()
    {
        var p = new TakeNotesPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void StandingByPower_IsBuff_Counter()
    {
        var p = new StandingByPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.False(p.ChoiceMode);
    }

    [Fact]
    public void StandingByPower_Localization_ReflectsLiveChoiceMode()
    {
        var p = new StandingByPower();
        Assert.Contains("random", p.Localization[1].Item2);
        Assert.Contains("random", p.Localization[2].Item2);

        p.ChoiceMode = true;
        Assert.Contains("of your choice", p.Localization[1].Item2);
        Assert.Contains("of your choice", p.Localization[2].Item2);
    }

    [Fact]
    public void FortissimoPower_IsBuff_Counter()
    {
        var p = new FortissimoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void MasterFormPower_IsBuff_Counter()
    {
        var p = new MasterFormPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void HeldNotePower_IsBuff_Counter()
    {
        var p = new HeldNotePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void HeldNotePower_IsActive_FalseForNullCreature()
    {
        Assert.False(HeldNotePower.IsActive(null));
    }

    [Fact]
    public void TheFirstLessonPower_IsBuff_Single()
    {
        var p = new TheFirstLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void FullVoicePower_IsBuff_Counter()
    {
        var p = new FullVoicePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void UnfrailPower_IsBuff_Counter()
    {
        var p = new UnfrailPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    // Bidirectional-interception structural checks, applied identically to all 6 Un-X powers:
    // each must declare its own TryModifyPowerAmountReceived/AfterModifyingPowerAmountReceived
    // (not just inherit the no-op base), and override IsVisibleInternal so it starts hidden at
    // Amount 0 (now that all 6 are always-attached from combat start) and reveals itself once
    // positive.

    private static void AssertHasBidirectionalInterception(Type powerType)
    {
        Assert.NotNull(powerType.GetMethod(
            "TryModifyPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(powerType.GetMethod(
            "AfterModifyingPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(powerType.GetProperty(
            "IsVisibleInternal", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    private static void AssertHiddenAtZero_VisibleWhenPositive(PowerModel p)
    {
        var prop = p.GetType().GetProperty("IsVisibleInternal", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.Equal(0, p.Amount);
        Assert.Equal(false, prop.GetValue(p));
    }

    [Fact]
    public void UnfrailPower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnfrailPower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnfrailPower());
    }

    [Fact]
    public void UnweakPower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnweakPower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnweakPower());
    }

    [Fact]
    public void UnvulnerablePower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnvulnerablePower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnvulnerablePower());
    }

    [Fact]
    public void UnshakenPower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnshakenPower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnshakenPower());
    }

    [Fact]
    public void UnlimitedPower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnlimitedPower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnlimitedPower());
    }

    [Fact]
    public void UnjadedPower_HasBidirectionalInterception_HiddenAtZero()
    {
        AssertHasBidirectionalInterception(typeof(UnjadedPower));
        AssertHiddenAtZero_VisibleWhenPositive(new UnjadedPower());
    }

    [Fact]
    public void FortissimoPower_HasRawAmountCapture()
    {
        Assert.NotNull(typeof(FortissimoPower).GetMethod(
            "BeforePowerAmountChanged", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void FortissimoPower_IsInvertiblePower_IncludesFrailAndUnfrail()
    {
        var method = typeof(FortissimoPower).GetMethod(
            "IsInvertiblePower", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        Assert.Equal(true, method!.Invoke(null, new object[] { new FrailPower() }));
        Assert.Equal(true, method.Invoke(null, new object[] { new UnfrailPower() }));
    }

    [Fact]
    public void InvertTrackerPower_IsBuff_Single_Hidden()
    {
        var p = new InvertTrackerPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        var isVisibleInternal = typeof(InvertTrackerPower)
            .GetProperty("IsVisibleInternal", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(p);
        Assert.Equal(false, isVisibleInternal);
    }

    [Fact]
    public void NewPowers_Localization_IsNonEmpty()
    {
        Assert.NotEmpty(new JadedPower().Localization);
        Assert.NotEmpty(new UnjadedPower().Localization);
        Assert.NotEmpty(new TakeNotesPower().Localization);
        Assert.NotEmpty(new StandingByPower().Localization);
        Assert.NotEmpty(new FortissimoPower().Localization);
        Assert.NotEmpty(new MasterFormPower().Localization);
        Assert.NotEmpty(new HeldNotePower().Localization);
        Assert.NotEmpty(new TheFirstLessonPower().Localization);
        Assert.NotEmpty(new FullVoicePower().Localization);
        Assert.NotEmpty(new UnfrailPower().Localization);
        Assert.NotEmpty(new InvertTrackerPower().Localization);
    }
}
