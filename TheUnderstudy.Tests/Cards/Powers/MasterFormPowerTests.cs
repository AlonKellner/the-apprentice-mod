using System.Reflection;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class MasterFormPowerTests
{
    // Now a plain per-play hook (grant Replay when an Attack/Skill without Replay is played), so no
    // static event subscription and no AfterApplied/AfterCombatEnd teardown is needed.
    [Fact]
    public void MasterFormPower_OverridesAfterCardPlayed()
    {
        var method = typeof(MasterFormPower).GetMethod(
            "AfterCardPlayed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(typeof(MasterFormPower), method?.DeclaringType);
    }

    [Fact]
    public void Localization_MentionsReplay_NotUnplayableOrPlannedOrTuned()
    {
        var p = new MasterFormPower();
        Assert.Contains(p.Localization, entry => entry.Item2.Contains("[gold]Replay[/gold]"));
        Assert.DoesNotContain(p.Localization, entry => entry.Item2.Contains("Unplayable"));
        Assert.DoesNotContain(p.Localization, entry => entry.Item2.Contains("Planned"));
        Assert.DoesNotContain(p.Localization, entry => entry.Item2.Contains("Tuned"));
    }
}
