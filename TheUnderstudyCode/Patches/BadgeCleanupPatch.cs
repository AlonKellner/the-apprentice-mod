using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Safety net for the Planned selection-index badges: their static state (SelectionIndexBadge) must never
// outlive the selection that created it. The per-selection ClearAll hooks handle normal completion and
// cancel, but a selection torn down another way — most notably quitting to the menu and continuing the
// run mid-selection — never runs CompleteSelection, so its badges were stranded and reappeared on the
// reloaded combat's cards. These two events clear the badges regardless of how a selection ends:
//   - the pile-grid select screen leaving the tree (completed, cancelled, or reload teardown);
//   - a fresh player hand entering the tree (a new or reloaded combat).
// ClearAll on an already-empty set is a no-op, so firing on non-Planned screens/hands is harmless.
[HarmonyPatch]
public static class BadgeCleanupPatch
{
    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), "_ExitTree")]
    [HarmonyPostfix]
    public static void GridScreenExit() => SelectionIndexBadge.ClearAll();

    [HarmonyPatch(typeof(NPlayerHand), "_Ready")]
    [HarmonyPostfix]
    public static void HandReady() => SelectionIndexBadge.ClearAll();
}
