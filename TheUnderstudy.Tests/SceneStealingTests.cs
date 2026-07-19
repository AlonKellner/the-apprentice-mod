using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure amount-math for the Swap mechanic. The registries and SwapEach itself need ModelDb/combat
// (not available in the bare test host — see AssemblyInfo.cs), so only the extracted math is
// unit-tested here; the full give/take flow is verified in-game.
public class SceneStealingTests
{
    // ComputeTransfer — how much of a debuff you hold moves per Swap X (capped at X and at holdings).

    [Fact]
    public void ComputeTransfer_HaveMoreThanX_TransfersX() =>
        Assert.Equal(2, SceneStealing.ComputeTransfer(have: 5, x: 2));

    [Fact]
    public void ComputeTransfer_HaveLessThanX_TransfersAllYouHave() =>
        Assert.Equal(3, SceneStealing.ComputeTransfer(have: 3, x: 5));

    [Fact]
    public void ComputeTransfer_HaveNone_TransfersNothing() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: 0, x: 2));

    [Fact]
    public void ComputeTransfer_NegativeHolding_NeverNegative() =>
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: -4, x: 2));

    // Give-negative half (sign-flip buffs Strength/Dexterity/Vigor): the magnitude of your NEGATIVE
    // portion given to enemies is ComputeTransfer(-have, x). E.g. -6 Vigor with Swap 6 gives 6 (you -> 0,
    // each enemy -6 more); capped at X; nothing to give when the holding is non-negative.
    [Fact]
    public void GiveNegative_MagnitudeIsNegatedHoldingCappedAtX()
    {
        Assert.Equal(6, SceneStealing.ComputeTransfer(have: -(-6), x: 6)); // -6 Vigor, Swap 6 -> give 6
        Assert.Equal(2, SceneStealing.ComputeTransfer(have: -(-6), x: 2)); // capped at Swap 2
        Assert.Equal(0, SceneStealing.ComputeTransfer(have: -3, x: 6));    // positive holding -> nothing given
    }

    // ComputeSteal — total taken across enemies, up to X (positive only) from each, summed.

    [Fact]
    public void ComputeSteal_SumsUpToXFromEachEnemy() =>
        Assert.Equal(2, SceneStealing.ComputeSteal(new[] { 3, 5 }, x: 1));

    [Fact]
    public void ComputeSteal_CapsAtEnemyHolding() =>
        Assert.Equal(3, SceneStealing.ComputeSteal(new[] { 1, 2 }, x: 5));

    [Fact]
    public void ComputeSteal_IgnoresNegativeAndZero() =>
        Assert.Equal(2, SceneStealing.ComputeSteal(new[] { -3, 0, 2 }, x: 5));

    [Fact]
    public void ComputeSteal_NoEnemies_StealsNothing() =>
        Assert.Equal(0, SceneStealing.ComputeSteal(System.Array.Empty<int>(), x: 3));
}
