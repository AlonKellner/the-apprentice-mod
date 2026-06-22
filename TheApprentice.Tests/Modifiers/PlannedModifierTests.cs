using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// These tests instantiate PlannedModifier which extends CardModifier (BaseLib/sts2 types).
// If those assemblies cannot be loaded in the test runner environment, individual tests will be skipped.
public class PlannedModifierTests
{
    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheApprentice:Planned", PlannedModifier.ModifierId);
    }

    // The following tests require CardModifier (BaseLib) which transitively loads sts2.dll.
    // sts2.dll is x86_64 and cannot be loaded by the arm64 test runner.
    // They are skipped here; the logic is covered indirectly via PlannedTrackerTests.
    private const string SkipReason = "Requires sts2.dll (x86_64) which cannot load in arm64 test runner";

    [Fact(Skip = SkipReason)]
    public void SequenceIndex_OnFirstModifier_IsZero()
    {
        PlannedTracker.ResetSequence();
        var mod = new PlannedModifier();
        Assert.Equal(0, mod.SequenceIndex);
    }

    [Fact(Skip = SkipReason)]
    public void SequenceIndex_OnThirdModifier_IsTwo()
    {
        PlannedTracker.ResetSequence();
        _ = new PlannedModifier();
        _ = new PlannedModifier();
        var third = new PlannedModifier();
        Assert.Equal(2, third.SequenceIndex);
    }

    [Fact(Skip = SkipReason)]
    public void MultipleModifiers_HaveStrictlyIncreasingIndexes()
    {
        PlannedTracker.ResetSequence();
        var a = new PlannedModifier();
        var b = new PlannedModifier();
        var c = new PlannedModifier();
        Assert.True(a.SequenceIndex < b.SequenceIndex);
        Assert.True(b.SequenceIndex < c.SequenceIndex);
    }

    [Fact(Skip = SkipReason)]
    public void AfterReset_SequenceIndexStartsFromZero()
    {
        PlannedTracker.ResetSequence();
        _ = new PlannedModifier();
        _ = new PlannedModifier();
        PlannedTracker.ResetSequence();
        var fresh = new PlannedModifier();
        Assert.Equal(0, fresh.SequenceIndex);
    }
}
