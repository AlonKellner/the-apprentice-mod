using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using Xunit;

namespace TheApprentice.Tests.Powers;

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
}
