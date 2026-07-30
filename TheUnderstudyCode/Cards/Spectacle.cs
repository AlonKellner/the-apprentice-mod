using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Rare "cash out your Tuned board" card: play every Tuned card in a random order (each independently
// retargeted, exactly like Remix does for the Planned queue), then strip Tuned from every card. The whole
// board fires while still Tuned, so each play gets the full stacks x Tuned-card-count bonus — and pays for
// it by ending the Tuned engine outright. No selection — it acts on all Tuned cards at once — so it needs
// no badge arming; the modifier changes are plain game-state mutations that run on every co-op client.
// Extends PlayAllPlannedCard for its once-per-turn guard: this card is a Skill, so it can itself be Tuned,
// and its own play-all would then AutoPlay itself and re-enter OnPlay forever — the identical hazard the
// Planned resolvers face when Planned+Stable. The guard marks the card resolved BEFORE the loop, which is
// what breaks the recursion (and per-instance marking breaks cross-card cycles too).
public class Spectacle : PlayAllPlannedCard
{
    public const string CardId = "TheUnderstudy:Spectacle";

    public Spectacle() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.Add);
        WithTip(UnderstudyKeywords.Tuned);
    }

    // What this one resolves is the Tuned board, not the Planned queue — nothing to cash out when empty.
    protected override bool HasQueueToResolve => TunedModifier.TunedCards(Owner).Any();

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        if (!TryBeginPlayAll(player)) return;

        // Locked once recorded and shuffled here — never re-fetched or re-sorted afterward. See
        // Remix.OnPlay for the full reasoning: a card in this list can itself be a Spectacle, which
        // resolves its OWN independently-captured board (possibly including cards that only became
        // Tuned during this pass) as a side effect of being played here. Every entry below is played
        // regardless; a card that gains Tuned mid-loop simply doesn't join THIS sequence.
        var tuned = TunedModifier.TunedCards(player).ToList();
        player.RunState.Rng.CombatCardSelection.Shuffle(tuned);
        Log.Info($"Spectacle.OnPlay: playing {tuned.Count} Tuned card(s) in shuffled order, each independently retargeted");

        foreach (var card in tuned)
        {
            // Does the card still exist? Real, not hypothetical: base-game Transform cards (e.g.
            // Begone) swap a card's original CardModel object out for a brand-new one, detaching
            // the original from every pile.
            if (card.Pile == null)
            {
                Log.Info($"Spectacle.OnPlay: {card.Id} is no longer in any pile — skipped");
                continue;
            }

            // A Tuned card locks itself Unplayable once played (TunedLockPower), so most of the board
            // is locked by the time this resolves. Clearing the lock is what makes "play ALL Tuned
            // cards" mean all of them — CardCmd.AutoPlay silently no-ops on an Unplayable card.
            if (card.TryGetModifier<UnplayableModifier>(out var locked))
                CardModifier.DirectModifiers(card).Remove(locked!);

            // Always pass null rather than reusing any target across cards: CardCmd.AutoPlay
            // itself rolls a fresh random living enemy for an AnyEnemy card whenever its target
            // argument is null, so this re-randomizes independently for every single card played.
            await CardCmd.AutoPlay(context, card, null, AutoPlayType.None, false, false);
        }

        // "Remove Tuned from ALL cards" — a fresh scan, not the captured list: a card played above may
        // have granted Tuned to itself or another card mid-loop (One-up, Practice), and the text
        // promises the board ends empty.
        foreach (var card in TunedModifier.TunedCards(player).ToList())
            if (card.TryGetModifier<TunedModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod!);
    }
}
