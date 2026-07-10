using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Shared, engine-independent state that couples the Planned appliers to the selection-index badge
// patches (PlannedGridSelectionPatch / PlannedHandSelectionPatch).
//
// Kept deliberately free of any Godot or Log.* calls: the Planned appliers (Foreshadow, Cue, etc.)
// call Arm()/OrderFor() from their OnPlay, and any Log.* on a path reachable from a bare-instantiated
// unit test crashes the xUnit host outright (Godot OS static ctor). See project memory
// "Log.* crashes the bare test host".
public static class PlannedSelectionState
{
    // Set by a Planned applier immediately before its CardSelectCmd call; consumed synchronously by
    // whichever selection screen opens next (grid Create postfix / hand SelectCards prefix). The
    // screen is constructed before the first await inside CardSelectCmd, so the flag is still set.
    // This is what gates the badge feature to Planned selections only — other CardSelectCmd callers
    // (Buildup, Safety Net, ...) never Arm(), so their screens stay untagged and unbadged.
    private static bool _armed;

    // The player's click order for the just-completed draw/discard grid selection, published by
    // PlannedGridSelectionPatch.CompleteSelectionPrefix and consumed by OrderFor. The base grid screen
    // returns an unordered HashSet, so this is the only channel carrying click order out to the applier.
    private static List<CardModel>? _gridOrder;

    public static void Arm() => _armed = true;

    public static bool ConsumeArmed()
    {
        bool wasArmed = _armed;
        _armed = false;
        return wasArmed;
    }

    public static void PublishGridOrder(IReadOnlyList<CardModel> order) => _gridOrder = order.ToList();

    // Returns the cards in the order Planned slots should be assigned. For a grid selection this is the
    // published click order (so the badge number the player saw becomes the real Planned #N); the check
    // that the published order matches `selected` guards against a stale publish. For a hand selection
    // nothing is published (`_gridOrder` is null) and `selected` is already in click order, so it passes
    // through unchanged — which also makes this a safe no-op fallback everywhere.
    public static IEnumerable<CardModel> OrderFor(IEnumerable<CardModel> selected)
    {
        var sel = selected as IReadOnlyCollection<CardModel> ?? selected.ToList();
        var order = _gridOrder;
        _gridOrder = null;
        if (order != null && order.Count == sel.Count && !sel.Except(order).Any())
            return order;
        return sel;
    }
}
