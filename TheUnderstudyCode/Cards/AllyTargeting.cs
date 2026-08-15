using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Target resolution for the AnyAlly co-op cards (Pass the Mic, Duet). A manual play sets cardPlay.Target
// from the targeting reticle, but a Planned/Tuned resolver AUTO-plays cards with no reticle. Two behaviours:
//
//  • Resolve(picked, owner) — the defensive path used inside the cards' own OnPlay. Uses the picked target
//    if it is a living ally, else a random living ally. Runs in lockstep OnPlay on every client and uses the
//    shared deterministic RunState.Rng, so the random pick never diverges. (With the resolver wiring below it
//    almost never has to fall back: the resolver already hands OnPlay a valid ally.)
//
//  • Prompt(owner) — the ordered-resolver path (Workshop/Showtime/Da Capo/Spectacle). At the moment the
//    resolver reaches an AnyAlly card it lets the ACTING player pick which ally via the combat targeting
//    reticle, synced across clients exactly like base-game MendRestSiteOption (heal-a-teammate): reserve a
//    choice id, the local player targets, the chosen player's NetId is broadcast, remote clients apply it.
//    The chaotic resolver (Remix) and the automatic turn-end resolver (Intermission) deliberately DON'T call
//    this — they hand null to AutoPlay, which picks a random ally itself. See the resolver call sites.
//
// Prompt short-circuits when there are 0-1 allies (no reticle at all), so in a 2-player co-op — where the one
// teammate is the only possible target — nothing pops up; the prompt only ever appears with 3+ players, which
// is exactly when the choice is meaningful.
public static class AllyTargeting
{
    // Other living player-creatures (never self). Deterministic order across clients (by NetId) so any
    // fallback pick is identical everywhere without consuming the RNG stream.
    private static List<Creature> LivingAllies(Creature self) =>
        (self.CombatState?.Allies ?? Enumerable.Empty<Creature>())
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != self)
            .OrderBy(c => c.Player!.NetId)
            .ToList();

    public static Creature? Resolve(Creature? picked, Player owner)
    {
        var allies = LivingAllies(owner.Creature);
        if (allies.Count == 0) return null;
        if (picked != null && allies.Contains(picked)) return picked;
        return owner.RunState.Rng.CombatTargets.NextItem(allies);
    }

    // Synced ally prompt for the ordered resolvers. Returns the chosen ally, or null only when the caster has
    // no living ally at all (AutoPlay then no-ops the card, which is correct — an ally card with no ally).
    public static async Task<Creature?> Prompt(Player owner)
    {
        var self = owner.Creature;
        var allies = LivingAllies(self);
        if (allies.Count == 0) return null;
        if (allies.Count == 1) return allies[0]; // only one possible target — skip the reticle entirely

        // Both the acting client and every remote reserve the same choice id at the same logical point, then
        // exactly one side (the acter) resolves it and broadcasts; the others wait for that broadcast.
        var sync = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = sync.ReserveChoiceId(owner);

        if (LocalContext.IsMe(owner))
        {
            Creature? chosen = null;
            try
            {
                chosen = await RunReticle(self);
            }
            catch
            {
                // A UI failure must never leave remote clients waiting forever — fall through to the
                // deterministic fallback and still broadcast a choice below.
                chosen = null;
            }
            if (chosen == null || !allies.Contains(chosen)) chosen = allies[0];
            sync.SyncLocalChoice(owner, choiceId, PlayerChoiceResult.FromPlayerId(chosen.Player!.NetId));
            return chosen;
        }
        else
        {
            var id = (await sync.WaitForRemoteChoice(owner, choiceId)).AsPlayerId();
            var chosen = id.HasValue ? owner.RunState.GetPlayer(id.Value)?.Creature : null;
            if (chosen == null || !allies.Contains(chosen)) chosen = allies[0];
            return chosen;
        }
    }

    // Runs the same combat targeting reticle a manually-played AnyAlly card uses (SingleCreatureTargeting):
    // AnyAlly restricts selectable nodes to other players, and combat creature nodes handle both mouse and
    // controller natively, so no per-node focus wiring is needed. Local-only — only the acting client reaches
    // this (gated by LocalContext.IsMe), so touching UI singletons here is safe.
    private static async Task<Creature?> RunReticle(Creature self)
    {
        var tm = NTargetManager.Instance;
        var start = NCombatRoom.Instance?.CreatureNodes.FirstOrDefault(n => n.Entity == self)?.GlobalPosition
                    ?? Vector2.Zero;
        var mode = NControllerManager.Instance?.IsUsingDirectionalNavigation == true
            ? TargetMode.Controller
            : TargetMode.ClickMouseToTarget;
        tm.StartTargeting(TargetType.AnyAlly, start, mode, () => false, null);
        return NodeToCreature(await tm.SelectionFinished());
    }

    private static Creature? NodeToCreature(Node? node) => node switch
    {
        NCreature c => c.Entity,
        NMultiplayerPlayerState p => p.Player.Creature,
        _ => null,
    };
}
