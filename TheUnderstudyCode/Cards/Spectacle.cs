using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Rare "cash out your Tuned board" card: play every Tuned card in the SAME order the Tuned counter lists
// them (pile-enumeration order — see TunedCounterPower), then strip Tuned from every card. Deterministic
// and single-target like Showtime (not shuffled + per-card retargeted like Remix): all cards funnel into
// one player-picked enemy, re-targeted only if that enemy dies mid-pass. The whole board fires while still
// Tuned, so each play gets the full stacks x Tuned-card-count bonus — and pays for it by ending the Tuned
// engine outright. No selection — it acts on all Tuned cards at once — so it needs no badge arming; the
// modifier changes are plain game-state mutations that run on every co-op client.
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

    // Like Showtime: require an enemy reticle only when the Tuned board actually has a single-target
    // attack to feed it — an empty board, or one of only AoE/self/no-target cards, plays with no prompt.
    // Owner throws on a canonical (not-yet-instantiated) card model, so fall back to the constructor-seeded
    // value there (bare-construction tests, card library previews, etc).
    public override TargetType TargetType =>
        IsMutable
            ? (TunedModifier.TunedQueueNeedsEnemyTarget(Owner) ? TargetType.AnyEnemy : TargetType.None)
            : base.TargetType;

    // What this one resolves is the Tuned board, not the Planned queue — nothing to cash out when empty.
    protected override bool HasQueueToResolve => TunedModifier.TunedCards(Owner).Any();

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        if (!TryBeginPlayAll(player)) return;

        var combatState = player.Creature.CombatState!;

        // Locked once recorded here — never re-fetched or re-sorted. Played in the Tuned counter's own
        // order (TunedModifier.TunedCards, the pile-enumeration order TunedCounterPower lists), NOT
        // shuffled. See Remix.OnPlay for the full reasoning on the locked list: a card in it can itself be
        // a Spectacle, which resolves its OWN independently-captured board (possibly including cards that
        // only became Tuned during this pass) as a side effect of being played here. Every entry below is
        // played regardless; a card that gains Tuned mid-loop simply doesn't join THIS sequence.
        var tuned = TunedModifier.TunedCards(player).ToList();
        Log.Info($"Spectacle.OnPlay: playing {tuned.Count} Tuned card(s) in Tuned-counter order");

        var currentTarget = cardPlay.Target;
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

            // Single shared target, Showtime-style: re-roll a fresh random living enemy only if the
            // current one has died partway through the board, so all single-target cards funnel into
            // the player's chosen enemy while it lives.
            if (card.TargetType == TargetType.AnyEnemy && (currentTarget == null || currentTarget.IsDead))
            {
                var previousTarget = currentTarget;
                currentTarget = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
                Log.Info($"Spectacle.OnPlay: target {previousTarget?.LogName ?? "(none)"} is no longer available — " +
                          $"re-targeted to {currentTarget?.LogName ?? "(none)"}");
            }

            // AnyAlly cards (Pass the Mic / Duet) prompt for the ally inside their OWN play, so the reticle
            // is visibly that card's and the pause is handled safely there. AutoPlayOrdered flags the play so
            // the card prompts; every other card keeps our enemy target. See AllyTargeting.
            await AllyTargeting.AutoPlayOrdered(context, card, currentTarget);
        }

        // "Remove Tuned from ALL cards" — a fresh scan, not the captured list: a card played above may
        // have granted Tuned to itself or another card mid-loop (One-up, Practice), and the text
        // promises the board ends empty.
        foreach (var card in TunedModifier.TunedCards(player).ToList())
            if (card.TryGetModifier<TunedModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod!);
    }
}
