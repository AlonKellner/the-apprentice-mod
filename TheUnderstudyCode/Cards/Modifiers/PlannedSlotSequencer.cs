using System;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// Pure slot-numbering core for PlannedModifier, deliberately kept free of any Godot/CardModel/Log
// dependency so its "never repeats, always +1, resyncs only on a token change" invariant can be
// unit-tested directly (PlannedModifier itself cannot be, since it needs a live ModelDb/Godot
// object graph a bare xUnit host can't construct, and Log.* crashes that host outright).
public class PlannedSlotSequencer
{
    private object? _lastToken;
    private int _next;

    // token identifies "which combat" (pass ICombatState in production — any reference type works
    // here, which is what keeps this class engine-independent). resyncStart computes the starting
    // point (existing max slot + 1) but is only ever invoked the moment token changes from the
    // previously-seen one — never on the per-call hot path — so a caller can make it scan live
    // state (protecting a mid-combat save/reload from repeating an already-restored slot) without
    // that scan ever running more than once per combat.
    public int Next(object token, Func<int> resyncStart)
    {
        if (!ReferenceEquals(token, _lastToken))
        {
            _lastToken = token;
            _next = resyncStart();
        }
        return _next++;
    }
}
