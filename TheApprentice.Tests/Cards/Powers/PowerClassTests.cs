using MegaCrit.Sts2.Core.Entities.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;
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
    public void MethodToTheMadnessPower_IsBuff_Single()
    {
        var p = new MethodToTheMadnessPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void VirtuosoPower_IsBuff_Single()
    {
        var p = new VirtuosoPower();
        Assert.Equal(PowerType.Buff, p.Type);
        Assert.Equal(PowerStackType.Single, p.StackType);
    }

    [Fact]
    public void AllPowers_Localization_IsNonEmpty()
    {
        Assert.NotEmpty(new InTheZonePower().Localization);
        Assert.NotEmpty(new ObsessionPower().Localization);
        Assert.NotEmpty(new MethodToTheMadnessPower().Localization);
        Assert.NotEmpty(new VirtuosoPower().Localization);
    }
}
