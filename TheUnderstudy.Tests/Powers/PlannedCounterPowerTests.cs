using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Powers;

public class PlannedCounterPowerTests
{
    [Fact]
    public void PlannedModifier_HasChangedEvent()
    {
        var ev = typeof(PlannedModifier).GetEvent("Changed", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(ev);
    }

    [Fact]
    public void PlannedModifier_Changed_CanFireWithSubscriber()
    {
        bool called = false;
        void handler() => called = true;
        PlannedModifier.Changed += handler;
        // Field-like events can only be invoked from inside the declaring class;
        // invoke the backing delegate via reflection.
        var field = typeof(PlannedModifier).GetField(
            "Changed", BindingFlags.NonPublic | BindingFlags.Static);
        var del = (Action?)field?.GetValue(null);
        del?.Invoke();
        PlannedModifier.Changed -= handler;
        Assert.True(called);
    }

    [Fact]
    public void PlannedCounterPower_OverridesAfterApplied()
    {
        var method = typeof(PlannedCounterPower).GetMethod(
            "AfterApplied",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(typeof(PlannedCounterPower), method?.DeclaringType);
    }

    [Fact]
    public void PlannedCounterPower_OverridesAfterCombatEnd()
    {
        var method = typeof(PlannedCounterPower).GetMethod(
            "AfterCombatEnd",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(typeof(PlannedCounterPower), method?.DeclaringType);
    }

    [Fact]
    public void BuildPlanList_EmptyList_ReturnsEmptyString()
    {
        Assert.Equal("", PlannedCounterPower.BuildPlanList([]));
    }

    [Fact]
    public void BuildPlanList_SingleCard_ShowsNumberedEntry()
    {
        var result = PlannedCounterPower.BuildPlanList(["Contemplate"]);
        Assert.Contains("#1", result);
        Assert.Contains("Contemplate", result);
    }

    [Fact]
    public void BuildPlanList_ThreeCards_NumberedInOrder()
    {
        var result = PlannedCounterPower.BuildPlanList(["Contemplate", "Groove", "Rehearsal"]);
        Assert.Contains("#1", result);
        Assert.Contains("#2", result);
        Assert.Contains("#3", result);
        Assert.True(result.IndexOf("Contemplate") < result.IndexOf("Groove"));
        Assert.True(result.IndexOf("Groove") < result.IndexOf("Rehearsal"));
    }

    [Fact]
    public void PlannedCounterPower_Localization_MentionsPlanned()
    {
        var p = new PlannedCounterPower();
        Assert.Contains(p.Localization, entry => entry.Item2.Contains("Planned"));
    }

    [Fact]
    public void BuildPlanList_ReorderedTitles_ProduceDifferentString()
    {
        var ab = PlannedCounterPower.BuildPlanList(["Alpha", "Beta"]);
        var ba = PlannedCounterPower.BuildPlanList(["Beta", "Alpha"]);
        Assert.NotEqual(ab, ba);
    }
}
