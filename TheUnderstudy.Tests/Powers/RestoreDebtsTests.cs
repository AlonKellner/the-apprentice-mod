using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Powers;

// Pure math behind the enemy-cleanup fixes for Swap/Invert. The live hooks (Toad Thorns, The Lost /
// The Forgotten Possess, Hunter Killer Tender) need a real combat and are verified in-game.
public class RestoreDebtsTests
{
    [Theory]
    // debt, amount => newDebt, paid
    [InlineData(3, 3, 0, 3)]   // smog -3, Invert 3 -> nothing left to give back
    [InlineData(3, 1, 2, 1)]   // partial Invert -> the rest is still owed
    [InlineData(2, 5, 0, 2)]   // Invert more than owed -> only the debt is paid, the rest flows on
    [InlineData(0, 4, 0, 0)]   // nothing owed
    [InlineData(3, 0, 3, 0)]   // nothing inverted
    public void Pay_CapsAtDebt(int debt, int amount, int expDebt, int expPaid)
    {
        var (newDebt, paid) = RestoreDebts.Pay(debt, amount);
        Assert.Equal(expDebt, newDebt);
        Assert.Equal(expPaid, paid);
    }

    [Theory]
    // before, after => owed
    [InlineData(0, -1, 1)]     // the normal Tender hit
    [InlineData(2, 1, 1)]      // spending positive Strength still counts as taken
    [InlineData(0, 0, 0)]      // Pulled Punch absorbed it
    [InlineData(0, 1, 0)]      // My Own Lesson turned it into a gain -> nothing owed
    public void LandedLoss_IsWhatActuallyLeft(int before, int after, int expected) =>
        Assert.Equal(expected, RestoreDebts.LandedLoss(before, after));

    [Theory]
    // amount, allowNegative, hasExisting => drop
    [InlineData(-5, false, false, true)]   // Toad takes away Thorns that Swap already stole
    [InlineData(-5, false, true, false)]   // existing Thorns: ModifyAmount removes it at <= 0
    [InlineData(-2, true, false, false)]   // Strength/Dexterity/Vigor may go negative
    [InlineData(5, false, false, false)]   // a normal gain
    public void PowerNeverNegative_DropsOnlyFreshNegatives(int amount, bool allowNegative, bool hasExisting, bool expected) =>
        Assert.Equal(expected, PowerNeverNegativePatch.ShouldDrop(amount, allowNegative, hasExisting));
}
