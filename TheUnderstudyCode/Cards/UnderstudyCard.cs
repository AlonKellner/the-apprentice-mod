using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

[Pool(typeof(TheUnderstudyCardPool))]
public abstract class UnderstudyCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true)
    : ConstructedCardModel(cost, type, rarity, target, showInCardLibrary)
{
    // Portrait resolution order: (1) bespoke per-card art if it exists, else (2) a single shared
    // per-card-type placeholder — card_portraits/placeholders/{attack,skill,power}.png — so every
    // card of a type draws from ONE editable image (edit it + republish to update them all at once),
    // else (3) the base game's blank missing-portrait default. Bespoke per-card art, once added,
    // always wins over the type placeholder.
    private string TypePlaceholderName => Type switch
    {
        CardType.Attack => "attack",
        CardType.Power => "power",
        _ => "skill",
    };

    private string? TypePlaceholderPortrait =>
        $"placeholders/{TypePlaceholderName}.png".CardImagePath();

    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath()
            ?? TypePlaceholderPortrait ?? MissingPortraitPath;

    public override string? CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath()
            ?? TypePlaceholderPortrait;

    // Call in subclass constructors to register the dynamic Tuned hover tip. The lambda is
    // evaluated at hover time with the live card, so it reads the current TunedModifier.Stacks —
    // returns nothing when Tuned hasn't been applied yet (no tip clutter on fresh cards).
    protected void WithTunedTip()
    {
        WithTips(card =>
        {
            if (!card.TryGetModifier<TunedModifier>(out var mod) || mod.Stacks <= 0)
                return Enumerable.Empty<IHoverTip>();
            int s = mod.Stacks;
            return new IHoverTip[]
            {
                new HoverTip(
                    new LocString("card_keywords", "THEUNDERSTUDY-TUNED.title"),
                    $"If this deals damage or grants [gold]Block[/gold], increase it by {s} for each card with [gold]Tuned[/gold]."
                )
            };
        });
    }

    private static readonly PropertyInfo InvertibleTipDescriptionProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Description))!;

    // Use instead of a plain WithTip(typeof(X)) when referencing one of the 5 invertible base
    // powers (Weak/Vulnerable/Frail/Strength/Dexterity). WithTip(typeof(X)) resolves through
    // HoverTipFactory.FromPower<T> -> ModelDb.Power<T>().GetDumbHoverTip(), a fully static/canonical
    // lookup with no notion of "which card is asking" — InvertibleBasePowerTooltipPatch (a Harmony
    // patch on the live PowerModel.HoverTips getter, gated on the CURRENT creature carrying
    // InvertTrackerPower) only covers hovering an actual applied power icon on a creature mid-combat;
    // it can never reach this path, and couldn't distinguish "an Understudy card is asking" from any
    // other character's card doing the same even if it could. Baking "Invertible" into the tip here
    // instead needs no runtime player/creature state at all, so it's correct in hand, reward screens,
    // deck view, and the Compendium — with or without an active run, not just mid-combat.
    // HoverTip is a sealed record struct we don't own with a private Description setter, so mutating
    // the boxed instance via reflection (same technique as InvertibleBasePowerTooltipPatch) is what
    // lets us append the suffix — never cast the IHoverTip reference to (HoverTip) before the
    // SetValue call, or it unboxes into a throwaway copy and the mutation is lost.
    protected void WithInvertibleTip(Type powerType)
    {
        WithTips(_ =>
        {
            IHoverTip tip = HoverTipFactory.FromPower(ModelDb.DebugPower(powerType));
            InvertibleTipDescriptionProperty.SetValue(tip, ((HoverTip)tip).Description + " [gold]Invertible[/gold].");
            return new IHoverTip[] { tip };
        });
    }

    // Same as BaseLib's WithPower<T>, but WITHOUT the auto-added power-description hover tip. Used by
    // Power cards whose own card text already states the effect in plain mechanical language, so the
    // power's tooltip would only duplicate the card description. The PowerVar<T> itself is still added
    // (it's what CommonActions.Apply<T> reads for the amount) — only the redundant tip is dropped.
    // Lesson cards that use flavour text keep the real WithPower<T> so their tooltip explains it.
    protected void WithPowerNoTip<T>(int baseVal, int upgrade = 0) where T : PowerModel =>
        WithVar(new PowerVar<T>(baseVal).WithUpgrade<PowerVar<T>>(upgrade));

    // The frozen configuration captured the first time this card is observed Stable; null means it is
    // not (yet) Stable. Deep (modifier state + local keywords) via StableEnforcer, so restore undoes
    // in-place mutation (e.g. an emptied Planned slot list), not just add/remove.
    private StableState? _stableSnapshot;

    // Whether we've subscribed to this card's own KeywordsChanged event (for immediate reversion of
    // base-game keyword edits like Ethereal). Cleared per combat.
    private bool _stableKeywordWatch;

    // The pre-Planned mechanic (starting a combat already queued) — revived from the original,
    // now-deleted Apprentice character's ApprenticeCard.IsPrePlanned, scoped to just the handful
    // of B cards that override this (see ApplyPrePlannedIfNeeded). Manually attaches
    // PlannedModifier with sentinel slot -1 (always sorts first in PlannedModifier.GetSorted, and
    // never collides with Apply's own auto-incrementing max+1 numbering) rather than calling
    // PlannedModifier.Apply, since Apply always assigns a fresh non-negative slot.
    public virtual bool IsPrePlanned => false;

    // True once this card has actually been pre-Planned for the current combat — reset only at
    // BeforeCombatStart. Guards against ApplyPrePlannedIfNeeded re-firing every time this card
    // re-enters a combat pile (AfterCardEnteredCombat isn't a one-shot "entered combat" hook — it
    // fires on every pile transition, including moving to Discard right after a "Play all Planned"
    // resolver's RemoveSlot call has just emptied the modifier out). Without this, a card starts
    // each combat Planned exactly once as intended, but the instant it's actually played and its
    // slot consumed, the very next pile transition would see "no PlannedModifier present" and
    // silently re-queue it — self-perpetuating, unlike Motif's genuinely deliberate self-Plan.
    private bool _prePlannedThisCombat;

    private void ApplyPrePlannedIfNeeded()
    {
        if (!IsPrePlanned) return;
        if (_prePlannedThisCombat) return;
        if (Pile?.Type.IsCombatPile() != true) return;
        if (this.TryGetModifier<PlannedModifier>(out _)) return;

        _prePlannedThisCombat = true;
        CardModifier.AddModifier<PlannedModifier>(this);
        this.TryGetModifier<PlannedModifier>(out var mod);
        // BaseLib shallow-clones modifier prototypes, so a freshly-attached instance shares its
        // SequenceIndices/VisualBySeq with every other clone until reinitialized — same reason
        // PlannedModifier.Apply always calls this for a brand-new attachment.
        mod!.ReinitCollections();
        mod.SequenceIndices.Add(-1);
        if (!this.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(this);
        PlannedModifier.InvokeChanged();
        PlannedModifier.RefreshVisualIndices(PlannedModifier.RelevantCards(Owner));
    }

    // The pre-Tuned mechanic (starting a combat already carrying Tuned 1) — same shape as
    // IsPrePlanned above, for "big one-off moment" B cards that should already be primed to lock
    // after their first play rather than needing a card-side grant.
    public virtual bool IsPreTuned => false;

    // True once this card has actually been pre-Tensified for the current combat — reset only
    // at BeforeCombatStart, same reasoning as _prePlannedThisCombat (AfterCardEnteredCombat fires
    // on every pile transition, not just the first).
    private bool _preTunedThisCombat;

    private void ApplyPreTunedIfNeeded()
    {
        if (!IsPreTuned || _preTunedThisCombat) return;
        if (Pile?.Type.IsCombatPile() != true) return;
        if (this.TryGetModifier<TunedModifier>(out _)) return;

        _preTunedThisCombat = true;
        TunedModifier.Apply(this, CombatState!, Owner!.Piles.SelectMany(p => p.Cards));
    }

    public override Task BeforeCombatStart()
    {
        var t = base.BeforeCombatStart();
        _prePlannedThisCombat = false;
        _preTunedThisCombat = false;
        // Reset (not just lazily set) here: a previous combat's snapshot/StableModifier grant
        // must not leak into a new one — MaybeSnapshotIfStable only ever sets _stableSnapshot
        // once it's null, so without this reset a printed-Stable card would only ever get
        // snapshotted on its very first combat, never refreshed on later ones.
        _stableSnapshot = null;
        EnforceStableNow();
        ApplyPrePlannedIfNeeded();
        ApplyPreTunedIfNeeded();
        return t;
    }

    // Auto-attach the shared PlannedCounterPower so the queue UI badge is visible whenever
    // Workshop queues cards, and the hidden InvertTrackerPower so Invert can react to
    // enemy-inflicted (not just self-applied) invertible debuffs and perform its bidirectional
    // debuff/buff cancellation for all 6 pairs (see InvertTrackerPower for why that logic lives
    // there rather than on each Un-X power). (Take Notes' "debuff cleared" detection used to need a
    // similar hidden tracker here, but now lives in DebuffClearOnRemovePatch — see DebuffClearNotifier.)
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        EnforceStableNow();
        if (player != Owner) return;
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
        if (!player.Creature.Powers.Any(p => p is InvertTrackerPower))
            await PowerCmd.Apply<InvertTrackerPower>(context, player.Creature, 1m, player.Creature, null, false);
        // Sole owner of the Tuned->Unplayable lock (see TunedLockPower). Hidden observer of every
        // card play, so it locks colorless/non-Understudy Tuned cards too — which the old per-card
        // attach in AfterCardPlayed (gated on cardPlay.Card == this) could never reach.
        if (!player.Creature.Powers.Any(p => p is TunedLockPower))
            await PowerCmd.Apply<TunedLockPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    // Restore on every card-play and turn boundary so no window exists where a Stable card
    // appears modified — covers enemy-applied effects (Ethereal, etc.) that slip past the
    // CanApplyTo guards.
    public override Task AfterCardEnteredCombat(CardModel triggeredBy)
    {
        EnforceStableNow();
        ApplyPrePlannedIfNeeded();
        ApplyPreTunedIfNeeded();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        EnforceStableNow();

        // Planned is only ever removed by an explicit "remove Planned" effect or by a "Play all
        // Planned" resolver (Showtime/DaCapo/Workshop/Remix) consuming the exact slot it's
        // resolving. A card that's simply playable (Unplayable freed some other way, e.g. by
        // Unwind/Confidence/StartOver) just plays normally when clicked manually — its own Planned
        // slot(s) are untouched and it stays queued to auto-play later too.
        //
        // The Tuned->Unplayable lock used to live here, but only fired for cards deriving from
        // UnderstudyCard (colorless Tuned cards escaped it). It now lives in TunedLockPower, a hidden
        // player power that observes every card play (see AfterPlayerTurnStartLate).
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        EnforceStableNow();
        return Task.CompletedTask;
    }

    // The single Stable-enforcement entry point, called at every significant combat event (combat
    // start, card entered/played, both sides' turn start, side turn end) plus — for immediate reaction
    // to modifications from any source — this card's own KeywordsChanged event and the ApplyInternal
    // Harmony patch (StableEnforcementPatch). The first time the card is observed Stable it takes the
    // deep snapshot (whether printed, or granted mid-combat by e.g. Final Draft on another card's OnPlay)
    // and starts watching keyword edits; every call then reconciles the live card back to that frozen
    // config via StableEnforcer.Restore. Deep restore undoes in-place mutation (e.g. an emptied Planned
    // slot list), so a Stable Planned card keeps its Planned through a queue resolution.
    private void EnforceStableNow()
    {
        if (_stableSnapshot == null)
        {
            if (!this.IsStable()) return;
            _stableSnapshot = StableEnforcer.Capture(this);
            if (!_stableKeywordWatch)
            {
                KeywordsChanged += OnStableKeywordsChanged;
                _stableKeywordWatch = true;
            }
        }
        if (StableEnforcer.Restore(this, _stableSnapshot))
            RefreshStablePlannedVisuals();
    }

    // Fires the instant this card's local keywords are edited (e.g. base-game Ethereal from Hex/Music
    // Box) — revert immediately. Guarded against re-entry from Restore's own AddKeyword/RemoveKeyword.
    private void OnStableKeywordsChanged()
    {
        if (_stableSnapshot == null || StableEnforcer.Enforcing) return;
        if (StableEnforcer.Restore(this, _stableSnapshot))
            RefreshStablePlannedVisuals();
    }

    // Called by StableEnforcementPatch the instant any BaseLib modifier is added to this card. Strip-only
    // (removes the addition if its type isn't part of the frozen config) — deliberately never re-adds,
    // so it can't re-lock an Unplayable that a Planned-queue resolver just stripped to auto-play a Stable
    // card mid-play. Full reconciliation (resets/re-adds) still happens at the next EnforceStableNow hook.
    internal void RejectForeignModifierIfStable(CardModifier justAdded)
    {
        if (_stableSnapshot == null || StableEnforcer.Enforcing) return;
        // Allow a type the frozen config already contains (e.g. re-adding an Unplayable a resolver
        // stripped); only strip a genuinely foreign type.
        if (_stableSnapshot.Modifiers.Any(m => m.modifier.GetType() == justAdded.GetType())) return;
        CardModifier.DirectModifiers(this).Remove(justAdded);
    }

    // Planned slot state may have just been restored; re-sync the global visual index badges. Reads
    // Owner (throws on a canonical card), so guard on IsMutable.
    private void RefreshStablePlannedVisuals()
    {
        if (IsMutable && this.TryGetModifier<PlannedModifier>(out _))
            PlannedModifier.RefreshVisualIndices(PlannedModifier.RelevantCards(Owner));
    }

    // Turn start for BOTH sides — the enforcement point covering "after enemy actions". (Overridden only
    // by Powers elsewhere, never by a card subclass, so adding it here is safe.)
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        EnforceStableNow();
        return Task.CompletedTask;
    }

    // Stop watching keyword changes and drop the snapshot so nothing leaks into the next combat.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_stableKeywordWatch)
        {
            KeywordsChanged -= OnStableKeywordsChanged;
            _stableKeywordWatch = false;
        }
        _stableSnapshot = null;
        return Task.CompletedTask;
    }
}
