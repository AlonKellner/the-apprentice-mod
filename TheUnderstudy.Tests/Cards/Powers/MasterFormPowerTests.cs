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

    // NOT AfterRemoved: Creature.RemoveAllPowersInternalExcept (the bulk power wipe at combat end)
    // is documented to skip that hook entirely, so cleanup lives in AfterCombatEnd instead — see
    // MasterFormPower's own comment on why.
    [Fact]
    public void MasterFormPower_OverridesAfterCombatEnd()
    {
        var method = typeof(MasterFormPower).GetMethod(
            "AfterCombatEnd", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
