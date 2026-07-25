namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Shared, engine-independent state that gates the selection-index badge patches
// (PlannedGridSelectionPatch / PlannedHandSelectionPatch) to Planned selections.
//
// Kept deliberately free of any Godot or Log.* calls: the Planned appliers (Preview, Development, etc.)
// call Arm() from their OnPlay, and any Log.* on a path reachable from a bare-instantiated unit test
// crashes the xUnit host outright (Godot OS static ctor). See project memory
// "Log.* crashes the bare test host".
//
// NOTE (multiplayer): Planned slots are assigned in the SYNCED selection-result order (the ordered
// NetPlayerChoiceResult.combatCards list, identical on every client), NOT in the player's local grid
// click order. A previous "publish the click order and apply in it" channel was REMOVED because it was
// client-local UI state — remote clients never open the screen, so they applied in a different order
// and the same cards received different Planned slot numbers (a state-sync divergence). Appliers now
// simply iterate the CardSelectCmd result. Badges remain a client-local visual only (see the patches).
public static class PlannedSelectionState
{
    // Set by a Planned applier immediately before its CardSelectCmd call; consumed synchronously by
    // whichever selection screen opens next (grid Create postfix / hand SelectCards prefix). The
    // screen is constructed before the first await inside CardSelectCmd, so the flag is still set.
    // This is what gates the badge feature to Planned selections only — other CardSelectCmd callers
    // (Practice, Safety Net, ...) never Arm(), so their screens stay untagged and unbadged.
    private static bool _armed;

    public static void Arm() => _armed = true;

    public static bool ConsumeArmed()
    {
        bool wasArmed = _armed;
        _armed = false;
        return wasArmed;
    }
}
