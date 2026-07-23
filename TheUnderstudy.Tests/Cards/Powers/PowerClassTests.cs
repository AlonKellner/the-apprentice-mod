using System.Linq;
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
        var descriptions = LocText.Of(p).Where(e => e.Item1 == "description").Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("fewer", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnlimitedPower_Localization_MentionsHandFull()
    {
        var p = new UnlimitedPower();
        var descriptions = LocText.Of(p).Where(e => e.Item1 == "description").Select(e => e.Item2);
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
        Assert.NotEmpty(LocText.Of(new PlannedCounterPower()));
        Assert.NotEmpty(LocText.Of(new UnweakPower()));
        Assert.NotEmpty(LocText.Of(new UnvulnerablePower()));
        Assert.NotEmpty(LocText.Of(new LimitedPower()));
        Assert.NotEmpty(LocText.Of(new UnlimitedPower()));
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

    // Upgrading Standing By reduces its cost rather than switching how freed cards are chosen, so
    // there is one power and the selection is always random — no player-choice variant exists.
    [Fact]
    public void StandingByPower_Localization_IsRandom()
    {
        var p = new BalancedPower();
        Assert.Equal("Balanced", LocText.Of(p)[0].Item2);
        Assert.Contains("random", LocText.Of(p)[1].Item2);
        Assert.Contains("random", LocText.Of(p)[2].Item2);
    }

    [Fact]
    public void StandingBy_HasNoPlayerChoiceVariant()
    {
        var powers = typeof(BalancedPower).Assembly.GetTypes()
            .Where(t => t.Name.StartsWith("Balanced") && typeof(PowerModel).IsAssignableFrom(t))
            .Select(t => t.Name).ToList();
        Assert.Equal(new[] { "BalancedPower" }, powers);
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
        Assert.NotEmpty(LocText.Of(new JadedPower()));
        Assert.NotEmpty(LocText.Of(new UnjadedPower()));
        Assert.NotEmpty(LocText.Of(new CryingOutLoudPower()));
        Assert.NotEmpty(LocText.Of(new BalancedPower()));
        Assert.NotEmpty(LocText.Of(new DoubleTimePower()));
        Assert.NotEmpty(LocText.Of(new MasterFormPower()));
        Assert.NotEmpty(LocText.Of(new HeldNotePower()));
        Assert.NotEmpty(LocText.Of(new TheFirstLessonPower()));
        Assert.NotEmpty(LocText.Of(new AnotherBrickPower()));
        Assert.NotEmpty(LocText.Of(new UnfrailPower()));
        Assert.NotEmpty(LocText.Of(new InvertTrackerPower()));
        Assert.NotEmpty(LocText.Of(new UndoomPower()));
    }

    [Fact]
    public void FinalLessonPower_IsBuff_Counter()
    {
        var p = new FinalLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
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
        Assert.NotEmpty(LocText.Of(p));
    }

    // Proves the swap logic correctly stayed centralized on InvertTrackerPower rather than
    // splitting into a second, competing TryModifyPowerAmountReceived listener (which would race
    // InvertTrackerPower for interception order with no reliable way to control who wins).
    [Fact]
    public void MyOwnLessonPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(MyOwnLessonPower));

    [Fact]
    public void InvertiblePair_ExposesApplyBuffAndDebuffSide()
    {
        // Reward/Punish resolution goes through pair.ApplyBuffSide / pair.ApplyDebuffSide (registry-driven).
        Assert.NotNull(typeof(InvertiblePair).GetMethod("ApplyBuffSide", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(typeof(InvertiblePair).GetMethod("ApplyDebuffSide", BindingFlags.Public | BindingFlags.Instance));
    }

    [Fact]
    public void SecondLessonPower_IsBuff_Single()
    {
        var p = new SecondLessonPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void SecondLessonPower_IsInstanced_SoASecondPlayStandsUpASecondLesson()
    {
        // Instanced makes PowerCmd.FindExistingInstanceForStacking return null, so a second play
        // builds a second power with its own Orders instead of stacking onto the first.
        Assert.Equal(PowerInstanceType.Instanced, new SecondLessonPower().InstanceType);
    }

    [Fact]
    public void RewardedPower_IsBuff_Counter()
    {
        var p = new RewardedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void PunishedPower_IsDebuff_Counter()
    {
        var p = new PunishedPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    // Both counters apply themselves on their own turn-start hook. They are singletons fed by any
    // number of Instanced Lessons, so owning the application is what makes it happen exactly once
    // per turn — resolving it from SecondLessonPower would apply the accumulated amount once per
    // live Lesson, and avoiding that would take explicit cross-instance coordination.
    [Fact]
    public void RewardedPower_AppliesItselfOnTurnStart() =>
        Assert.Contains(
            typeof(RewardedPower).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            m => m.Name == "AfterPlayerTurnStartLate");

    [Fact]
    public void PunishedPower_AppliesItselfOnTurnStart() =>
        Assert.Contains(
            typeof(PunishedPower).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            m => m.Name == "AfterPlayerTurnStartLate");

    [Fact]
    public void CallSheetPower_IsBuff_Counter()
    {
        var p = new MusePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
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
        Assert.NotEmpty(LocText.Of(p));
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
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void MuscleMemoryPower_IsActive_FalseForNullCreature() =>
        Assert.False(MuscleMemoryPower.IsActive(null));

    [Fact]
    public void ReverbPower_IsBuff_Counter()
    {
        var p = new ReverbPower();
        Assert.Equal(PowerType.Buff, p.Type);
        // Counter (not Single) so the stacking turn-count is visible.
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void ReverbPower_IsActive_FalseForNullCreature() =>
        Assert.False(ReverbPower.IsActive(null));

    [Fact]
    public void ReverbPower_SelfRemovesAtTurnEnd() =>
        Assert.NotNull(typeof(ReverbPower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void EnjoyTheRidePower_IsBuff_Counter()
    {
        var p = new EnjoyTheRidePower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    // Now a one-shot end-of-turn Invert (like Bright Side's hook but self-removing), no longer
    // the old reactive "next modified debuff" AfterPowerAmountChanged trigger.
    [Fact]
    public void EnjoyTheRidePower_InvertsAtTurnEnd() =>
        Assert.NotNull(typeof(EnjoyTheRidePower).GetMethod(
            "BeforeSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

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
        var descriptions = LocText.Of(new ApathyPower())
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
        var descriptions = LocText.Of(new OneTakePower())
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
    public void AdLibPower_Localization_MentionsInvert()
    {
        var descriptions = LocText.Of(new BrightSidePower())
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Invert", d, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AdLibPower_OverridesAfterPlayerTurnStart() =>
        Assert.NotNull(typeof(BrightSidePower).GetMethod(
            "AfterPlayerTurnStart", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StereoPower_IsBuff_Counter()
    {
        var p = new StereoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
    }

    [Fact]
    public void StereoPower_Localization_MentionsVigor()
    {
        var descriptions = LocText.Of(new StereoPower())
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Vigor", d));
    }

    [Fact]
    public void StereoPower_OverridesModifyPowerAmountGivenMultiplicative() =>
        Assert.NotNull(typeof(StereoPower).GetMethod(
            "ModifyPowerAmountGivenMultiplicative", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_IsBuff_Single()
    {
        var p = new IntermissionPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void StageManagerPower_Localization_MentionsPlanned()
    {
        var descriptions = LocText.Of(new IntermissionPower())
            .Where(e => e.Item1 == "description" || e.Item1 == "smartDescription")
            .Select(e => e.Item2);
        Assert.All(descriptions, d => Assert.Contains("Planned", d));
    }

    [Fact]
    public void StageManagerPower_OverridesAfterCardPlayed() =>
        Assert.NotNull(typeof(IntermissionPower).GetMethod(
            "AfterCardPlayed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_OverridesAfterPlayerTurnStartLate() =>
        Assert.NotNull(typeof(IntermissionPower).GetMethod(
            "AfterPlayerTurnStartLate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void StageManagerPower_OverridesAfterSideTurnEnd() =>
        Assert.NotNull(typeof(IntermissionPower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    // ── Swap-mechanic powers ────────────────────────────────────────────────────────────────

    [Fact]
    public void TensionPower_IsDebuff_Counter()
    {
        var p = new TensionPower();
        Assert.Equal(PowerType.Debuff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void TensionPower_DamagesAtTurnEnd() =>
        Assert.NotNull(typeof(TensionPower).GetMethod(
            "BeforeSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void UntensionPower_IsBuff_Counter()
    {
        var p = new UntensionPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void UntensionPower_HealsAtTurnEnd() =>
        Assert.NotNull(typeof(UntensionPower).GetMethod(
            "BeforeSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void UntaintedPower_IsBuff_Counter()
    {
        var p = new UntaintedPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    // Untainted reduces incoming Attack damage (mirror of base Tainted) and clears at the opponent's
    // turn end — so it overrides both ModifyDamageAdditive and AfterSideTurnEnd.
    [Fact]
    public void UntaintedPower_OverridesModifyDamageAdditiveAndTurnEnd()
    {
        Assert.NotNull(typeof(UntaintedPower).GetMethod(
            "ModifyDamageAdditive", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.NotNull(typeof(UntaintedPower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    // Undoom — buff mirror of base Doom. Heals up to X at the end of the opponent's turn (AfterSideTurnEnd),
    // does not remove itself. Its cancellation with Doom is centralized on InvertTrackerPower (no local
    // interception overrides).
    [Fact]
    public void UndoomPower_IsBuff_Counter()
    {
        var p = new UndoomPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Counter, p.StackType);
        Assert.NotEmpty(LocText.Of(p));
    }

    [Fact]
    public void UndoomPower_HealsAtOpponentTurnEnd() =>
        Assert.NotNull(typeof(UndoomPower).GetMethod(
            "AfterSideTurnEnd", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    [Fact]
    public void UndoomPower_HasNoInterceptionOverrides() => AssertHasNoInterceptionOverrides(typeof(UndoomPower));
}
