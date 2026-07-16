using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Abstract base for the "Play all Planned" resolver cards (Workshop, Showtime, DaCapo, Remix). Its once-per-turn
// guard both caps the effect (per card instance) and — because it marks the card resolved BEFORE the queue
// is played — breaks the infinite recursion that occurs when such a resolver is itself Planned+Stable:
// playing it AutoPlays its own Planned slot, re-entering OnPlay; the guard makes that nested self-play a
// no-op. Per-instance marking also breaks cross-card mutual recursion (any cycle must re-enter an
// already-marked instance). Abstract, so it's never registered as a card (mirrors UnderstudyStarterRelic).
//
// Note this is deliberately NOT the base-game Fetch approach (querying finished card-play history): the
// outer play isn't "finished" while the nested self-play runs, so a history check would still read
// "first time this turn" and fail to stop the recursion. Marking before the queue-play is what works.
public abstract class PlayAllPlannedCard(
    int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
    : UnderstudyCard(cost, type, rarity, target, showInCardLibrary)
{
    private bool _resolvedThisTurn;

    // Call at the very top of OnPlay: returns false (card does nothing) if the play-all already resolved
    // this turn; otherwise marks it resolved and returns true. Marking happens before the queue is played.
    protected bool BeginPlayAllThisTurn()
    {
        if (_resolvedThisTurn) return false;
        _resolvedThisTurn = true;
        return true;
    }

    protected void ResetPlayAllThisTurn() => _resolvedThisTurn = false;

    // ── Diagnostics + once-per-turn invariant ─────────────────────────────────────────────────────────
    // Combat-only wrapper the resolvers call from OnPlay instead of BeginPlayAllThisTurn directly (which
    // stays pure/Log-free for the unit tests). Logs every resolve/block attempt with the concrete resolver
    // type, a stable per-instance id, the turn number, and the guard outcome — so a future runaway shows
    // exactly which instances resolved, how often, and when. Also asserts the once-per-turn contract the
    // guard is supposed to provide: if the SAME instance resolves the queue twice in one turn, the guard
    // was bypassed and the invariant fires with all the identifying detail.
    private static ICombatState? _diagCombat;
    private static readonly HashSet<PlayAllPlannedCard> _resolvedThisTurnGlobal = new();

    protected bool TryBeginPlayAll(Player player)
    {
        int turn = player.Creature.Player?.PlayerCombatState?.TurnNumber ?? -1;
        int id = RuntimeHelpers.GetHashCode(this);

        if (!BeginPlayAllThisTurn())
        {
            Log.Info($"{GetType().Name}#{id}[turn {turn}]: play-all BLOCKED — already resolved this turn (once-per-turn guard held)");
            return false;
        }

        var combat = player.Creature.CombatState;
        if (!ReferenceEquals(combat, _diagCombat))
        {
            _diagCombat = combat;
            _resolvedThisTurnGlobal.Clear();
        }
        bool firstThisTurn = _resolvedThisTurnGlobal.Add(this);
        Log.Info($"{GetType().Name}#{id}[turn {turn}]: play-all RESOLVING (firstThisTurn={firstThisTurn})");
        Invariants.Check(firstThisTurn, nameof(PlayAllPlannedCard) + "." + nameof(TryBeginPlayAll),
            $"{GetType().Name} instance #{id} resolved a play-all more than once in turn {turn} — the " +
            "once-per-turn guard was bypassed (e.g. the resolved flag was reset mid-turn)");
        return true;
    }

    // Glow gold only while there's a plan to resolve AND this card hasn't spent its once-per-turn yet.
    protected override bool ShouldGlowGoldInternal =>
        !_resolvedThisTurn && PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    public override Task BeforeCombatStart()
    {
        _resolvedThisTurn = false;
        _resolvedThisTurnGlobal.Clear();
        return base.BeforeCombatStart();
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        await base.AfterPlayerTurnStartLate(context, player);
        if (player == Owner)
        {
            ResetPlayAllThisTurn();
            // New turn: clear the once-per-turn diagnostic/invariant registry. All resolver instances'
            // turn-start hooks run before any card is played this turn, so clearing here (possibly several
            // times) always precedes the turn's first resolution.
            _resolvedThisTurnGlobal.Clear();
        }
    }
}
