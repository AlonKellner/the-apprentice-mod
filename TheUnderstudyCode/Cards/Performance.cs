using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Performance : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Performance";

    public Performance() : base(0, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithVars(new CardsVar("Select", 2));
        WithTip(UnderstudyKeywords.Planned);
    }

    // Only require a target when the queue actually has a card that needs one — an empty plan,
    // or a plan of only AoE/self/no-target cards, should just play with no reticle. Owner throws
    // on a canonical (not-yet-instantiated) card model, so fall back to the constructor-seeded
    // value there (bare-construction tests, card library previews, etc).
    public override TargetType TargetType =>
        IsMutable
            ? (PlannedModifier.QueueNeedsEnemyTarget(PlannedModifier.RelevantCards(Owner)) ? TargetType.AnyEnemy : TargetType.None)
            : base.TargetType;

    // Glow gold while there are Planned cards to resolve — same cue as the other Planned resolvers
    // (CurtainCall/Encore/FinalBar), signalling that playing this now will play the queue.
    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var combatState = player.Creature.CombatState!;

        // Step 1: Play all currently-Planned cards in queue order, consuming each slot. This list
        // is locked once recorded here and never re-fetched or re-sorted — a card with two slots
        // is played twice. If one of these cards is itself a Planned-queue resolver (e.g. Remix),
        // playing it can resolve some of the OTHER entries still waiting in this same locked list
        // as a side effect of its own nested pass — that's fine and expected: every entry below
        // always gets played regardless, and the per-entry guards just avoid redoing (not
        // re-playing) work another resolver already did.
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        Log.Info($"Performance.OnPlay: playing {planned.Count} Planned slot(s)");
        var currentTarget = cardPlay.Target;
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            // Does the card still exist? The one case a locked-sequence entry is skipped outright.
            // Real, not hypothetical: base-game Transform cards (e.g. Begone, via CardCmd.Transform)
            // swap a card's original CardModel object out for a brand-new one, detaching the
            // original from every pile — if that original card was sitting in the Planned queue,
            // `card` here is a reference captured when the sequence was recorded above, never
            // re-validated until now, so it must be checked before acting on it.
            if (card.Pile == null)
            {
                Log.Info($"Performance.OnPlay: {card.Id} is no longer in any pile — skipped");
                continue;
            }

            // Is this Unplayable? Remove it if so — guarded, not asserted. RemoveSlot only clears
            // UnplayableModifier once ALL of a card's Planned slots are gone (correctly so in
            // general — a card with a remaining slot must still refuse a manual player click) but a
            // multi-slot card (e.g. Planned #2 and #3) must still be playable on EACH of its own
            // plays in this loop, not just its last. CardCmd.AutoPlay silently no-ops
            // (MoveToResultPileWithoutPlaying, no error) if the card still carries
            // CardKeyword.Unplayable.
            if (card.TryGetModifier<UnplayableModifier>(out var stillUnplayable))
                CardModifier.DirectModifiers(card).Remove(stillUnplayable);

            // Is the Planned index still on the card? Remove it if so — guarded rather than
            // asserted, since a nested resolver played from elsewhere in this same locked sequence
            // (e.g. Remix, itself queued earlier in it) may have already removed it as part of its
            // own pass. Either way, this entry was recorded in the locked sequence, so it always
            // gets played below regardless.
            if (card.TryGetModifier<PlannedModifier>(out var stillPlanned) && stillPlanned.SequenceIndices.Contains(slotSeqIdx))
                PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);

            // If the enemy we were targeting has since died partway through the plan, re-target to
            // a fresh random living enemy for this and all subsequent AnyEnemy cards, until that one
            // also dies. Matches CardCmd.AutoPlay's own null-target fallback (same RNG stream), just
            // resolved here so it can be reused across multiple plays instead of re-rolling every time.
            if (card.TargetType == TargetType.AnyEnemy && (currentTarget == null || currentTarget.IsDead))
            {
                var previousTarget = currentTarget;
                currentTarget = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
                Log.Info($"Performance.OnPlay: target {previousTarget?.LogName ?? "(none)"} is no longer available — " +
                          $"re-targeted to {currentTarget?.LogName ?? "(none)"}");
            }

            await CardCmd.AutoPlay(context, card, currentTarget, AutoPlayType.None, false, false);
        }
        PlannedModifier.InvokeChanged();

        // Step 2: Apply Planned to 0-N cards selected from the discard pile (sets up next turn's queue).
        var maxSelect = IsUpgraded ? 3 : 2;
        PlannedSelectionState.Arm();
        var selectedRaw = await CardSelectCmd.FromCombatPile(
            context,
            PileType.Discard.GetPile(player),
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PERFORMANCE.selectionPrompt"), 0, maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c));

        if (selectedRaw == null) return;
        foreach (var card in PlannedSelectionState.OrderFor(selectedRaw))
            PlannedModifier.Apply(card, combatState);

        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, cardPlay.Card, false);
    }
}
