using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards;
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
        var p = new CryingOutLoudPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void StandingByPower_IsBuff_Counter()
    {
        var p = new BalancedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void StandingByChoicePower_IsBuff_Counter()
    {
        var p = new BalancedChoicePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    // Random (base) and choice (upgraded) are two independent powers that both present as the same
    // "Balanced" badge — same name and (via the shared base's override) same icon — differing
    // only in tooltip: "random attack or skill" vs "attack or skill of your choice".
    [Fact]
    public void StandingByPower_Localization_IsRandom()
    {
        var p = new BalancedPower();
        Assert.Equal("Balanced", p.Localization[0].Item2);
        Assert.Contains("random", p.Localization[1].Item2);
        Assert.Contains("random", p.Localization[2].Item2);
    }

    [Fact]
    public void StandingByChoicePower_Localization_IsChoice()
    {
        var p = new BalancedChoicePower();
        Assert.Equal("Balanced", p.Localization[0].Item2);
        Assert.Contains("of your choice", p.Localization[1].Item2);
        Assert.Contains("of your choice", p.Localization[2].Item2);
    }

    [Fact]
    public void StandingByPowers_ShareBaseAndIcon()
    {
        // Both derive from the shared base...
        Assert.IsAssignableFrom<BalancedPowerBase>(new BalancedPower());
        Assert.IsAssignableFrom<BalancedPowerBase>(new BalancedChoicePower());
        // ...and neither re-declares the icon path, so the base's single override drives both
        // (guarantees an identical badge icon without invoking Godot's ResourceLoader here).
        var randomIcon = typeof(BalancedPower).GetProperty("CustomPackedIconPath")!.DeclaringType;
        var choiceIcon = typeof(BalancedChoicePower).GetProperty("CustomPackedIconPath")!.DeclaringType;
        Assert.Equal(typeof(BalancedPowerBase), randomIcon);
        Assert.Equal(typeof(BalancedPowerBase), choiceIcon);
    }

    [Fact]
    public void DoubleTimePower_IsBuff_Counter()
    {
        var p = new DoubleTimePower();
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
        var p = new AnotherBrickPower();
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

    // Cancellation logic now lives centrally on InvertTrackerPower (see below), not on the 6 Un-X
    // powers themselves — each Un-X power is a plain buff again: no TryModifyPowerAmountReceived/
    // AfterModifyingPowerAmountReceived of its own, and no IsVisibleInternal override (so the base
    // "always visible once attached" applies, matching Strength/Dexterity — they're never attached
    // while empty anymore, so there's nothing to hide).

    private static void AssertHasNoInterceptionOverrides(Type powerType)
    {
        Assert.Null(powerType.GetMethod(
            "TryModifyPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(powerType.GetMethod(
            "AfterModifyingPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(powerType.GetProperty(
            "IsVisibleInternal", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void UnfrailPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnfrailPower));

    [Fact]
    public void UnweakPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnweakPower));

    [Fact]
    public void UnvulnerablePower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnvulnerablePower));

    [Fact]
    public void UnshakenPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnshakenPower));

    [Fact]
    public void UnlimitedPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnlimitedPower));

    [Fact]
    public void UnjadedPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UnjadedPower));

    [Fact]
    public void InvertTrackerPower_HasBidirectionalInterception()
    {
        Assert.NotNull(typeof(InvertTrackerPower).GetMethod(
            "TryModifyPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(typeof(InvertTrackerPower).GetMethod(
            "AfterModifyingPowerAmountReceived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void DoubleTimePower_HasRawAmountCapture()
    {
        Assert.NotNull(typeof(DoubleTimePower).GetMethod(
            "BeforePowerAmountChanged", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void DoubleTimePower_IsInvertiblePower_IncludesFrailAndUnfrail()
    {
        var method = typeof(DoubleTimePower).GetMethod(
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
        Assert.NotEmpty(new CryingOutLoudPower().Localization);
        Assert.NotEmpty(new BalancedPower().Localization);
        Assert.NotEmpty(new DoubleTimePower().Localization);
        Assert.NotEmpty(new MasterFormPower().Localization);
        Assert.NotEmpty(new HeldNotePower().Localization);
        Assert.NotEmpty(new TheFirstLessonPower().Localization);
        Assert.NotEmpty(new AnotherBrickPower().Localization);
        Assert.NotEmpty(new UnfrailPower().Localization);
        Assert.NotEmpty(new InvertTrackerPower().Localization);
    }

    [Fact]
    public void FinalLessonPower_IsBuff_Counter()
    {
        var p = new FinalLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void FinalLessonPower_OverridesModifyHpLostAfterOstyLate()
    {
        Assert.NotNull(typeof(FinalLessonPower).GetMethod(
            "ModifyHpLostAfterOstyLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void FinalLessonPower_OverridesBeforeSideTurnEnd()
    {
        Assert.NotNull(typeof(FinalLessonPower).GetMethod(
            "BeforeSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void MyOwnLessonPower_IsBuff_Single()
    {
        var p = new MyOwnLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    // Proves the swap logic correctly stayed centralized on InvertTrackerPower rather than
    // splitting into a second, competing TryModifyPowerAmountReceived listener (which would race
    // InvertTrackerPower for interception order with no reliable way to control who wins).
    [Fact]
    public void MyOwnLessonPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(MyOwnLessonPower));

    [Fact]
    public void ApplyBuffSide_And_ApplyDebuffSide_Exist()
    {
        Assert.NotNull(typeof(EmotionalExpression).GetMethod("ApplyBuffSide", BindingFlags.Public | BindingFlags.Static));
        Assert.NotNull(typeof(EmotionalExpression).GetMethod("ApplyDebuffSide", BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void SecondLessonPower_IsBuff_Single()
    {
        var p = new SecondLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void RewardedPower_IsBuff_Counter()
    {
        var p = new RewardedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void PunishedPower_IsDebuff_Counter()
    {
        var p = new PunishedPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    // Plain data-holder Counter powers — proves the per-turn Reward/Punish logic stayed
    // centralized on SecondLessonPower rather than each power independently reacting to its own
    // turn-start hook (which would make "Reward resolves first" depend on cross-power hook
    // iteration order, the same hazard already avoided for My Own Lesson).
    [Fact]
    public void RewardedPower_HasNoOverriddenHooks() =>
        Assert.DoesNotContain(
            typeof(RewardedPower).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            m => !m.IsSpecialName);

    [Fact]
    public void PunishedPower_HasNoOverriddenHooks() =>
        Assert.DoesNotContain(
            typeof(PunishedPower).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            m => !m.IsSpecialName);

    [Fact]
    public void CallSheetPower_IsBuff_Counter()
    {
        var p = new MusePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void CallSheetPower_OverridesAfterPlayerTurnStartLate() =>
        Assert.NotNull(typeof(MusePower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void PerfectionismPower_IsBuff_Counter()
    {
        var p = new PerfectionismPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void PerfectionismPower_OverridesAfterPlayerTurnStartLate() =>
        Assert.NotNull(typeof(PerfectionismPower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void MuscleMemoryPower_IsBuff_Single()
    {
        var p = new MuscleMemoryPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void MuscleMemoryPower_IsActive_FalseForNullCreature() =>
        Assert.False(MuscleMemoryPower.IsActive(null));

    [Fact]
    public void EncorePower_IsBuff_Single()
    {
        var p = new EncorePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void EncorePower_IsActive_FalseForNullCreature() =>
        Assert.False(EncorePower.IsActive(null));

    [Fact]
    public void EncorePower_SelfRemovesAtTurnEnd() =>
        Assert.NotNull(typeof(EncorePower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void SwingPower_IsBuff_Single()
    {
        var p = new SwingPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void SwingPower_InvertsEachTurnStart() =>
        Assert.NotNull(typeof(SwingPower).GetMethod(
            "AfterPlayerTurnStart", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void EnjoyTheRidePower_IsBuff_Counter()
    {
        var p = new EnjoyTheRidePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(p.Localization);
    }

    [Fact]
    public void EnjoyTheRidePower_ReactsToPowerAmountChange() =>
        Assert.NotNull(typeof(EnjoyTheRidePower).GetMethod(
            "AfterPowerAmountChanged", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void PulledPunchPower_IsBuff_Counter()
    {
        var p = new ApathyPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void PulledPunchPower_Localization_MentionsInvertible()
    {
        var descriptions = new ApathyPower().Localization
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("invertible", d, StringComparison.OrdinalIgnoreCase));
    }

    // Pulled Punch is a plain marker/counter power: the actual debuff reduction is folded into
    // InvertTrackerPower's single interception (gated on this power's presence), and the pure
    // reduction math is covered by PulledPunchPowerTests.Dampen_*.
    [Fact]
    public void PulledPunchPower_IsPlainMarker_NoOwnAmountHook() =>
        Assert.Null(typeof(ApathyPower).GetMethod(
            "AfterPowerAmountChanged", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void OneTakePower_IsBuff_Counter()
    {
        var p = new OneTakePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void OneTakePower_Localization_MentionsUnplayable()
    {
        var descriptions = new OneTakePower().Localization
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Unplayable", d));
    }

    [Fact]
    public void OneTakePower_OverridesTryModifyEnergyCostInCombat() =>
        Assert.NotNull(typeof(OneTakePower).GetMethod(
            "TryModifyEnergyCostInCombat", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void OneTakePower_OverridesAfterCardPlayed() =>
        Assert.NotNull(typeof(OneTakePower).GetMethod(
            "AfterCardPlayed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void AdLibPower_IsBuff_Counter()
    {
        var p = new BrightSidePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void AdLibPower_Localization_MentionsInvertible()
    {
        var descriptions = new BrightSidePower().Localization
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("invertible", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AdLibPower_OverridesBeforeSideTurnEnd() =>
        Assert.NotNull(typeof(BrightSidePower).GetMethod(
            "BeforeSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void CrescendoPower_IsBuff_Counter()
    {
        var p = new CrescendoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void CrescendoPower_Localization_MentionsVigor()
    {
        var descriptions = new CrescendoPower().Localization
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Vigor", d));
    }

    [Fact]
    public void CrescendoPower_OverridesModifyPowerAmountGivenMultiplicative() =>
        Assert.NotNull(typeof(CrescendoPower).GetMethod(
            "ModifyPowerAmountGivenMultiplicative", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_IsBuff_Single()
    {
        var p = new VenuePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void StageManagerPower_Localization_MentionsPlanned()
    {
        var descriptions = new VenuePower().Localization
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Planned", d));
    }

    [Fact]
    public void StageManagerPower_OverridesAfterCardPlayed() =>
        Assert.NotNull(typeof(VenuePower).GetMethod(
            "AfterCardPlayed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_OverridesAfterPlayerTurnStartLate() =>
        Assert.NotNull(typeof(VenuePower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_OverridesAfterSideTurnEnd() =>
        Assert.NotNull(typeof(VenuePower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
}
