using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Shared target resolution for the AnyAlly co-op cards (Pass the Mic, Duet). A manual play sets
// cardPlay.Target from the targeting reticle, but a Planned/Tuned resolver (Workshop, Showtime, Da Capo,
// Remix, Spectacle) AUTO-plays cards with no reticle: it passes null (a None-target resolver like Workshop)
// or its own enemy target (an AnyEnemy resolver like Showtime), and its per-card retarget logic only
// handles AnyEnemy — so an auto-played AnyAlly card previously received a null/enemy target and did nothing.
//
// This makes those cards robust: use the picked target only if it is actually a living ally; otherwise fall
// back to a random living ally (in 2-player co-op that is simply the one other player). Uses RunState.Rng
// so the pick is identical on every client — it runs inside OnPlay, which executes in lockstep across
// clients, so it must never diverge. Null when there is no living ally.
public static class AllyTargeting
{
    public static Creature? Resolve(Creature? picked, Player owner)
    {
        var self = owner.Creature;
        var allies = self.CombatState?.Allies
            .Where(c => c != null && c.IsAlive && c != self)
            .Select(c => c!)
            .ToList();
        if (allies == null || allies.Count == 0) return null;
        if (picked != null && allies.Contains(picked)) return picked;
        return owner.RunState.Rng.CombatTargets.NextItem(allies);
    }
}
