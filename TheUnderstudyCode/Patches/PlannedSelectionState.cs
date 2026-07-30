using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Shared, engine-independent state that gates the selection-index badge patches
// (PlannedGridSelectionPatch / PlannedHandSelectionPatch) to Planned selections.
//
// Kept deliberately free of any Godot or Log.* calls: the Planned appliers (Preview, Development, etc.)
// call Arm() from their OnPlay, and any Log.* on a path reachable from a bare-instantiated unit test
// crashes the xUnit host outright (Godot OS static ctor). See project memory
// "Log.* crashes the bare test host".
//
// NOTE (multiplayer): in MULTIPLAYER, Planned slots are assigned in the SYNCED selection-result order (the
// ordered NetPlayerChoiceResult.combatCards list, identical on every client), NOT the player's local grid
// click order. A previous "publish the click order and apply in it" channel was REMOVED because it was
// client-local UI state — remote clients never open the screen, so they applied in a different order and
// the same cards received different Planned slot numbers (a state-sync divergence).
//
// In SINGLE-PLAYER there is exactly one client, so honoring the local click order is deterministic and
// safe. So the click order is published ONLY when single-player (see PublishClickOrder), and appliers pass
// their selection through InClickOrder to reorder it to match the badges. In multiplayer the click order is
// never published, so InClickOrder is a no-op and the synced result order is used unchanged.
public static class PlannedSelectionState
{
    // Set by a Planned applier immediately before its CardSelectCmd call; consumed synchronously by
    // whichever selection screen opens next (grid Create postfix / hand SelectCards prefix). The
    // screen is constructed before the first await inside CardSelectCmd, so the flag is still set.
    // This is what gates the badge feature to Planned selections only — other CardSelectCmd callers
    // (Practice, Safety Net, ...) never Arm(), so their screens stay untagged and unbadged.
    private static bool _armed;

    // The single-player click order captured from the badge patches (null in multiplayer, or before any
    // selection). Consumed once by InClickOrder at apply time.
    private static List<CardModel>? _clickOrder;

    public static void Arm()
    {
        _armed = true;
        _clickOrder = null; // drop any stale order from a cancelled/previous selection
    }

    public static bool ConsumeArmed()
    {
        bool wasArmed = _armed;
        _armed = false;
        return wasArmed;
    }

    // Called by the badge patches on every selection change with the current click-ordered cards. Stored
    // only in single-player (the multiplayer safety gate). Overwrites each change, so the last publish
    // before the selection completes is the final click order.
    public static void PublishClickOrder(IReadOnlyList<CardModel> ordered, Player? owner)
    {
        bool singlePlayer = (owner?.RunState?.Players.Count ?? 1) <= 1;
        _clickOrder = singlePlayer ? ordered.ToList() : null;
    }

    // Reorders a selection result to match the captured single-player click order, then clears it. In
    // multiplayer (or with no captured order) returns the selection unchanged. Consumed once per selection.
    public static IReadOnlyList<CardModel> InClickOrder(IReadOnlyList<CardModel> selected)
    {
        if (_clickOrder == null) return selected;
        var ordered = ReorderByClickOrder(selected, _clickOrder);
        _clickOrder = null;
        return ordered;
    }

    // Pure, unit-testable reorder: cards named in `clickOrder` come first in that order (only those
    // actually in `selected`), followed by any remaining selected cards in their original order.
    public static List<T> ReorderByClickOrder<T>(IReadOnlyList<T> selected, IReadOnlyList<T> clickOrder)
    {
        var result = new List<T>();
        foreach (var c in clickOrder)
            if (selected.Contains(c) && !result.Contains(c))
                result.Add(c);
        foreach (var c in selected)
            if (!result.Contains(c))
                result.Add(c);
        return result;
    }
}
