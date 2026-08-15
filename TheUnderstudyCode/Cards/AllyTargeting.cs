using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Target resolution for the AnyAlly co-op cards (Pass the Mic, Duet). The ally choice lives inside each
// card's OWN OnPlay (via ResolveTarget), so when a resolver auto-plays it the selection reticle is visibly
// that card's — "who does Pass the Mic hand off to?" — not the resolver's.
//
// Three cases:
//  • Manual play — the targeting reticle already chose the ally in the UI before the action ran; use it.
//  • Ordered-resolver auto-play (Workshop/Showtime/Da Capo/Spectacle) — the resolver flags the play via
//    AutoPlayOrdered, so the card PROMPTS for the ally, paused SAFELY through the action framework.
//  • Remix / Intermission auto-play — unflagged, so the card takes a random living ally (AutoPlay already
//    seeded one), matching "Remix is the random resolver".
//
// The prompt mirrors CardSelectCmd's synced-choice pattern EXACTLY: ReserveChoiceId, then
// SignalPlayerChoiceBegun to register the pause with the ActionExecutor, then the local reticle (or the
// remote wait), SyncLocalChoice, and finally SignalPlayerChoiceEnded. The SignalPlayerChoiceBegun/Ended
// brackets are what a previous version was missing — without them the raw reticle await ended up "Canceled
// while paused" and desynced. See [[project_reticle_in_action_desync]].
public static class AllyTargeting
{
    // Set true by an ORDERED resolver immediately around its AutoPlay of an AnyAlly card, telling that card
    // to prompt for its ally (vs. Remix/Intermission, which leave it false → random ally). ResolveTarget
    // reads AND clears it (consume-on-read) at the very top of the card's play — before any await that could
    // pause — so its value is identical on every client (set/read by the same lockstep code) and can never
    // linger across a pause to leak into another player's interleaved auto-play. AutoPlayOrdered's
    // save/restore additionally covers the case where the card never reaches ResolveTarget (e.g. Unplayable,
    // so AutoPlay no-ops before OnPlay). Vetted in MultiplayerStaticStateGuardTests.
    public static bool PromptAutoPlayedAlly;

    private static List<Creature> LivingAllies(Creature self) =>
        (self.CombatState?.Allies ?? Enumerable.Empty<Creature>())
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != self)
            .OrderBy(c => c.Player!.NetId)
            .ToList();

    // Auto-play one queued card for an ordered resolver. AnyAlly cards are flagged so they prompt for the
    // ally inside their own OnPlay (target passed as null → AutoPlay seeds a random-ally fallback in case
    // there's no prompt); every other card keeps the resolver's own (enemy) target.
    public static async Task AutoPlayOrdered(PlayerChoiceContext context, CardModel card, Creature? resolverTarget)
    {
        if (card.TargetType != TargetType.AnyAlly)
        {
            await CardCmd.AutoPlay(context, card, resolverTarget, AutoPlayType.None, false, false);
            return;
        }
        var prev = PromptAutoPlayedAlly;
        PromptAutoPlayedAlly = true;
        try { await CardCmd.AutoPlay(context, card, null, AutoPlayType.None, false, false); }
        finally { PromptAutoPlayedAlly = prev; }
    }

    // Decide an AnyAlly card's target, called at the top of the card's OnPlay. Null only when the caster has
    // no living ally (the card then no-ops, which is correct — an ally card with no ally).
    public static async Task<Creature?> ResolveTarget(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Consume the ordered-resolver flag immediately — BEFORE any await below can pause this action — so
        // it can never leak into another player's interleaved auto-play while we're paused for our own choice.
        bool prompt = PromptAutoPlayedAlly;
        PromptAutoPlayedAlly = false;

        var owner = cardPlay.Card.Owner;
        var allies = LivingAllies(owner.Creature);
        if (allies.Count == 0) return null;

        // Manual play: the reticle already picked a valid ally in the UI before this action ran.
        if (!cardPlay.IsAutoPlay && cardPlay.Target is { } picked && allies.Contains(picked))
            return picked;

        // Ordered-resolver auto-play: prompt for the ally now, as part of THIS card's play.
        if (prompt)
            return await Prompt(context, owner, allies);

        // Remix / Intermission / stray: a random living ally. Runs in lockstep OnPlay with the shared RNG,
        // so the pick is identical on every client.
        if (cardPlay.Target is { } t && allies.Contains(t)) return t;
        return owner.RunState.Rng.CombatTargets.NextItem(allies);
    }

    private static async Task<Creature?> Prompt(PlayerChoiceContext context, Player owner, List<Creature> allies)
    {
        if (allies.Count == 1) return allies[0]; // only one possible target — no pause, no reticle

        var sync = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = sync.ReserveChoiceId(owner);
        // Register the pause with the action framework BEFORE the long reticle/remote await, exactly as
        // CardSelectCmd does — this is what lets the ActionExecutor pause and cleanly resume the action.
        await context.SignalPlayerChoiceBegun(owner, PlayerChoiceOptions.None);
        Creature chosen;
        try
        {
            if (LocalContext.IsMe(owner))
            {
                Creature? picked = null;
                try { picked = await RunReticle(owner.Creature); }
                catch { picked = null; } // a UI failure must still broadcast a choice, never hang remotes
                chosen = picked != null && allies.Contains(picked) ? picked : allies[0];
                sync.SyncLocalChoice(owner, choiceId, PlayerChoiceResult.FromPlayerId(chosen.Player!.NetId));
            }
            else
            {
                var id = (await sync.WaitForRemoteChoice(owner, choiceId)).AsPlayerId();
                var remote = id.HasValue ? owner.RunState.GetPlayer(id.Value)?.Creature : null;
                chosen = remote != null && allies.Contains(remote) ? remote : allies[0];
            }
        }
        finally
        {
            await context.SignalPlayerChoiceEnded();
        }
        return chosen;
    }

    // The same combat targeting reticle a manually-played AnyAlly card uses (SingleCreatureTargeting):
    // AnyAlly restricts selectable nodes to other players, and combat creature nodes handle mouse and
    // controller natively. Local-only — only the acting client reaches this (gated by LocalContext.IsMe).
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
