using MegaCrit.Sts2.Core.Logging;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

// Runtime invariant checks for state that should always stay in lockstep (a cached
// tracking list vs. the live game state it mirrors, a visible counter vs. its
// underlying source of truth, a single-slot re-entrancy guard, etc). Hook dispatch
// in the engine has no try/catch around listener calls, so violations here must
// always log-and-continue — never throw.
public static class Invariants
{
    // General condition check. Returns the condition so hook call sites can
    // `if (!Invariants.Check(...)) return;` to bail out safely instead of acting on
    // state that's already known to be inconsistent.
    public static bool Check(bool condition, string context, string message)
    {
        if (!condition)
            Log.Error($"[Invariant] {context}: {message}");
        return condition;
    }

    // Convenience overload for the common "expected == actual" count comparison.
    public static bool CheckEqual(int expected, int actual, string context, string label)
    {
        bool ok = expected == actual;
        if (!ok)
            Log.Error($"[Invariant] {context}: {label} mismatch — expected {expected}, got {actual}.");
        return ok;
    }
}
