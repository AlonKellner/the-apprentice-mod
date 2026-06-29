using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests;

public class TensionHelperTests
{
    // ── ComputeTensionGain — pure math, testable in isolation ─────────────────

    [Fact]
    public void ComputeTensionGain_NoModifiers_ReturnsBaseAmount() =>
        Assert.Equal(5, TensionHelper.ComputeTensionGain(5, 0, 1, false));

    [Fact]
    public void ComputeTensionGain_TuningBonus1_AddsExtra1() =>
        Assert.Equal(6, TensionHelper.ComputeTensionGain(5, 1, 1, false));

    [Fact]
    public void ComputeTensionGain_TuningBonus2_AddsExtra2() =>
        Assert.Equal(7, TensionHelper.ComputeTensionGain(5, 2, 1, false));

    [Fact]
    public void ComputeTensionGain_CadenceDouble_DoublesBase() =>
        Assert.Equal(10, TensionHelper.ComputeTensionGain(5, 0, 2, false));

    [Fact]
    public void ComputeTensionGain_CadenceDoubleAndTuning1_DoublesFirst_ThenAddsBonus() =>
        Assert.Equal(11, TensionHelper.ComputeTensionGain(5, 1, 2, false));

    [Fact]
    public void ComputeTensionGain_CadenceTriple_TriplesBase() =>
        Assert.Equal(15, TensionHelper.ComputeTensionGain(5, 0, 3, false));

    [Fact]
    public void ComputeTensionGain_CadenceTripleAndTuning1_TriplesFirst_ThenAddsBonus() =>
        Assert.Equal(16, TensionHelper.ComputeTensionGain(5, 1, 3, false));

    [Fact]
    public void ComputeTensionGain_FortissimoTriple_TriplesBase() =>
        Assert.Equal(15, TensionHelper.ComputeTensionGain(5, 0, 1, true));

    [Fact]
    public void ComputeTensionGain_CadenceDoubleAndFortissimoTriple_BothApply() =>
        Assert.Equal(30, TensionHelper.ComputeTensionGain(5, 0, 2, true));

    [Fact]
    public void ComputeTensionGain_CadenceTripleAndFortissimoTriple_BothApply() =>
        Assert.Equal(45, TensionHelper.ComputeTensionGain(5, 0, 3, true));

    [Fact]
    public void ComputeTensionGain_AllModifiers_CadenceFirst_ThenTriple_ThenBonus() =>
        Assert.Equal(31, TensionHelper.ComputeTensionGain(5, 1, 2, true));

    // ── ComputePartialRemoval ─────────────────────────────────────────────────

    [Fact]
    public void ComputePartialRemoval_CurrentBelowCap_RemovesAll() =>
        Assert.Equal(3, TensionHelper.ComputePartialRemoval(3, 8));

    [Fact]
    public void ComputePartialRemoval_CurrentAboveCap_RemovesCap() =>
        Assert.Equal(8, TensionHelper.ComputePartialRemoval(10, 8));

    [Fact]
    public void ComputePartialRemoval_CurrentExactlyCap_RemovesCap() =>
        Assert.Equal(8, TensionHelper.ComputePartialRemoval(8, 8));

    [Fact]
    public void ComputePartialRemoval_Zero_ReturnsZero() =>
        Assert.Equal(0, TensionHelper.ComputePartialRemoval(0, 8));
}
