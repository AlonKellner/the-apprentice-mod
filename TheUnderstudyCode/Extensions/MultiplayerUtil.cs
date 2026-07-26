using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Platform;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

// Multiplayer helpers. Card/power OnPlay hooks run in lockstep on EVERY client for EVERY player's
// actions, so any side effect that should touch only "my screen" (client-local UI) must be gated to the
// local player — otherwise a remote player's play triggers it on my client too. The Planned-badge arming
// (PlannedSelectionState.Arm) is exactly this case: without gating, a remote applier stranded the badge
// flag on other clients, where it was later consumed by an unrelated local selection (Tuned) and drew
// rogue Planned badges.
//
// This gates PRESENTATION only. It must NEVER gate a game-state mutation — those are supposed to run on
// every client (that is what keeps the checksummed model graph in sync).
public static class MultiplayerUtil
{
    // True if `owner` is the player on THIS machine. Defaults to true when locality can't be determined
    // (headless test host or an uninitialised platform layer) so non-multiplayer behaviour is unchanged.
    //
    // Single-player is short-circuited to true: the sole player's NetId does NOT actually equal the
    // platform's local id (it is unset/zero in single-player while GetLocalPlayerId returns the Steam id),
    // so comparing them returned false and stranded every presentation gate — that is what silently
    // killed the Planned selection badges. The NetId comparison only matters in an actual co-op run
    // (more than one player), where a remote player's play must not trigger local-only UI on my client.
    public static bool IsLocalPlayer(Player? owner)
    {
        if (owner == null) return true;
        try
        {
            var players = owner.RunState?.Players;
            if (players == null || players.Count <= 1) return true; // single-player: trivially local
            return IsLocalPlayerId(owner.NetId, PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform));
        }
        catch
        {
            return true;
        }
    }

    // Pure core, unit-testable without the platform layer: the acting player is local iff their net id
    // matches the local player id.
    public static bool IsLocalPlayerId(ulong ownerNetId, ulong localPlayerId) => ownerNetId == localPlayerId;
}
