using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Silently auto-attached to the player at combat start (see UnderstudyCard.AfterPlayerTurnStartLate,
// alongside InvertTrackerPower/PlannedCounterPower). Sole owner of the Tuned->Unplayable lock:
// after a Tuned card finishes its final play, it becomes Unplayable so it can't be replayed.
//
// This used to live on UnderstudyCard.AfterCardPlayed, gated on `cardPlay.Card == this`, which only
// ran for cards deriving from UnderstudyCard — so a colorless/base-game card (e.g. Bolas) that
// received Tuned got the damage/block bonus but never locked. As an always-attached, always-hidden
// observer of EVERY card play, this power covers those cards too. It mirrors the base game's own
// idiom, where Exhaust/Ethereal/Retain/Unplayable are handled by the central play pipeline reading a
// keyword off the card, not by the card reacting to itself.
//
// AfterCardPlayedLate (not AfterCardPlayed): CombatState.IterateHookListeners enumerates a creature's
// Powers before its Cards, so a Power's AfterCardPlayed runs before the just-played card's own
// AfterCardPlayed — too early to see a stack the card grants ITSELF this play (Da Capo). The Late
// pass runs after every listener's AfterCardPlayed, so IsFinalTunedPlay sees that self-granted stack.
public class TunedLockPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override List<(string, string)> Localization => new PowerLoc("Tuned Lock", "", "");

    // Pure decision (no combat state, no ModelDb, no Log.*): a card should be locked exactly when this
    // is its final Tuned play and it isn't already locked. The decision is a function of (card, play)
    // only, with no card-type gate — which is what fixes colorless cards. Split out from the attach
    // below so it's unit-testable on bare cards (AddModifier<T> needs a populated ModelDb). Muscle
    // Memory immunity is NOT checked here — it's enforced centrally in
    // UnplayableModifier.OnInitialApplication, the single point every attach funnels through.
    public static bool ShouldLock(CardModel card, CardPlay cardPlay) =>
        TunedModifier.IsFinalTunedPlay(cardPlay)
        && !card.TryGetModifier<UnplayableModifier>(out _);

    public static void LockIfFinalTunedPlay(CardModel card, CardPlay cardPlay)
    {
        if (ShouldLock(card, cardPlay))
            CardModifier.AddModifier<UnplayableModifier>(card);
    }

    // Only player-deck cards ever carry TunedModifier, so IsFinalTunedPlay is inherently player-scoped
    // — no Owner check needed (and none wanted: Owner throws on canonical cards).
    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        LockIfFinalTunedPlay(cardPlay.Card, cardPlay);
        return Task.CompletedTask;
    }
}
