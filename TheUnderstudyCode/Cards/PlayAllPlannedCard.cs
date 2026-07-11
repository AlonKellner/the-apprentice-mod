using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Abstract base for the "Play all Planned" resolver cards (Curtain Call, Encore, Remix). Its once-per-turn
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

    // Glow gold only while there's a plan to resolve AND this card hasn't spent its once-per-turn yet.
    protected override bool ShouldGlowGoldInternal =>
        !_resolvedThisTurn && PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    public override Task BeforeCombatStart()
    {
        _resolvedThisTurn = false;
        return base.BeforeCombatStart();
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        await base.AfterPlayerTurnStartLate(context, player);
        if (player == Owner) ResetPlayAllThisTurn();
    }
}
