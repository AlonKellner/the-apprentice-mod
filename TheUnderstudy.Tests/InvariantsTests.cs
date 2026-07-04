using TheUnderstudy.TheUnderstudyCode.Extensions;
using Xunit;

namespace TheUnderstudy.Tests;

// Only the passing (non-logging) path is covered here. The failure path calls into
// MegaCrit.Sts2.Core.Logging.Log, whose static constructor touches Godot's OS singleton —
// that crashes the bare test host outright (verified empirically; MegaCrit.Sts2.Core.TestSupport
// .TestMode.IsOn does not help), so it cannot be exercised in this harness at all. Verified
// in-game instead, same as every other Log.Error call in this codebase.
public class InvariantsTests
{
    [Fact]
    public void Check_TrueCondition_ReturnsTrue() =>
        Assert.True(Invariants.Check(true, "Context", "message"));

    [Fact]
    public void CheckEqual_MatchingCounts_ReturnsTrue() =>
        Assert.True(Invariants.CheckEqual(3, 3, "Context", "label"));
}
