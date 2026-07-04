using System.Reflection;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class MasterFormPowerTests
{
    [Fact]
    public void MasterFormPower_OverridesAfterApplied()
    {
        var method = typeof(MasterFormPower).GetMethod(
            "AfterApplied", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(typeof(MasterFormPower), method?.DeclaringType);
    }

    [Fact]
    public void MasterFormPower_OverridesAfterRemoved()
    {
        var method = typeof(MasterFormPower).GetMethod(
            "AfterRemoved", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(typeof(MasterFormPower), method?.DeclaringType);
    }

    [Fact]
    public void Localization_MentionsUnplayable_NotPlannedOrIntense()
    {
        var p = new MasterFormPower();
        Assert.Contains(p.Localization, entry => entry.Item2.Contains("[gold]Unplayable[/gold]"));
        Assert.DoesNotContain(p.Localization, entry => entry.Item2.Contains("Planned"));
        Assert.DoesNotContain(p.Localization, entry => entry.Item2.Contains("Intense"));
    }
}
