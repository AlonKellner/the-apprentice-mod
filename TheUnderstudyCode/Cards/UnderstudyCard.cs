using System;
using System.Collections;
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
using TheUnderstudy.TheUnderstudyCode.Patches;
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

    // (The per-card WithTunedTip() helper that used to live here is gone: TunedModifier.AddTips now
    // supplies the same tip from the modifier side, so it reaches every Tuned card — including
    // colorless and base-game ones — instead of only the two Basic cards that remembered to call it.)

    private static readonly PropertyInfo TipDescriptionProperty =
        typeof(HoverTip).GetProperty(nameof(HoverTip.Description))!;

    // Use instead of a plain WithTip(typeof(X)) when a card references a base-game power that the
    // Understudy's Invert/Swap mechanics act on — a debuff (Weak/Vulnerable/Frail/…) OR a buff
    // (Vigor/Strength/Dexterity/…). Invertible and Swappable are appended by the exact same
    // BasePowerTooltipSuffixPatch.MissingSuffix used by the live power-icon path (whose classification
    // derives from DoubleTimePower.IsInvertiblePower + SceneStealing's Swap registries), so a card's
    // keyword tip and an applied power icon always agree and can't drift from the mechanic.
    //
    // WithTip(typeof(X)) resolves through HoverTipFactory.FromPower<T> -> a fully static/canonical
    // lookup with no notion of "which card is asking"; the live-icon Harmony patch is gated on the
    // creature carrying InvertTrackerPower and so can never reach this canonical path. Baking the
    // suffix in here needs no runtime creature state, so it's correct in hand, reward screens, deck
    // view, and the Compendium, with or without an active run.
    //
    // Mod powers (Shaken/Tension/Un-pairs) already carry their suffix in their own PowerLoc, so
    // MissingSuffix is a no-op for them (they're not in the base classification sets, and it's
    // idempotent) — no duplication. HoverTip is a sealed record struct we don't own with a private
    // Description setter, so we mutate the boxed instance via reflection — never cast the IHoverTip
    // reference to (HoverTip) before the SetValue call, or it unboxes into a throwaway copy and the
    // mutation is lost.
    protected void WithMarkedTip(Type powerType)
    {
        WithTips(_ =>
        {
            var power = ModelDb.DebugPower(powerType);
            IHoverTip tip = HoverTipFactory.FromPower(power);
            string description = ((HoverTip)tip).Description;
            description += BasePowerTooltipSuffixPatch.MissingSuffix(power, description);
            TipDescriptionProperty.SetValue(tip, description);
            return new IHoverTip[] { tip };
        });
    }

    private static readonly FieldInfo ConstructedHoverTipsField =
        typeof(ConstructedCardModel).GetField("_hoverTips", BindingFlags.Instance | BindingFlags.NonPublic)!;

    // Same as BaseLib's WithPower<T>, but WITHOUT the auto-added power-description hover tip. Used by
    // Power cards whose own card text already states the effect in plain mechanical language, so the
    // power's tooltip would only duplicate the card description. The PowerVar<T> itself is still added
    // (it's what CommonActions.Apply<T> reads for the amount) — only the redundant tip is dropped.
    // The Second Lesson and The Final Lesson keep the real WithPower<T>, because their card text is
    // flavour rather than mechanics and needs the power tooltip to explain itself.
    //
    // Dropping the tip takes a removal rather than an omission: adding the var is what creates it.
    // BaseLib's WithVars (which WithVar forwards to) scans every var's runtime type and calls
    // WithTip(arg) for each generic type argument assignable to PowerModel — so PowerVar<T> tips
    // itself, and the earlier "just don't call WithTip" version of this helper silently did nothing.
    // WithTip appends exactly one TooltipSource per call, so removing the last entry removes precisely
    // the one this var just added. Guarded on the list actually having grown, so that if a future
    // BaseLib drops the auto-tip this degrades to a no-op instead of eating a real tip (or throwing on
    // an empty list). CardHoverTipCountTests fails loudly either way.
    protected void WithPowerNoTip<T>(int baseVal, int upgrade = 0) where T : PowerModel
    {
        var tips = (IList)ConstructedHoverTipsField.GetValue(this)!;
        int before = tips.Count;
        WithVar(new PowerVar<T>(baseVal).WithUpgrade<PowerVar<T>>(upgrade));
        if (tips.Count > before) tips.RemoveAt(tips.Count - 1);
    }

    // The frozen configuration captured the first time this card is observed Stable; null means it is
    // not (yet) Stable. Deep (modifier state + local keywords) via StableEnforcer, so restore undoes
    // in-place mutation (e.g. an emptied Planned slot list), not just add/remove.
    private StableState? _stableSnapshot;

    // Whether we've subscribed to this card's own change events — KeywordsChanged (for immediate reversion
    // of base-game keyword edits like Ethereal) and ReplayCountChanged (for direct BaseReplayCount writes
    // from Master Form / Hidden Gem / Sword Sage). Both are subscribed and dropped together, per combat.
    private bool _stableWatch;

    // The pre-Planned mechanic (starting a combat already queued) — scoped to the handful of B cards
    // that override this (Signature, upgraded Experience). The actual queuing is no longer per-card:
    // PrePlannedSetup runs once per combat and assigns every pre-Planned card a concrete, unique slot
    // via PlannedModifier's owner-scoped slot derivation in deck order (see AfterPlayerTurnStartLate below), so they
    // keep their deck position and take the lowest slots. (Previously each attached a shared sentinel
    // slot -1, so pre-Planned cards all overlapped with no distinct order.)
    public virtual bool IsPrePlanned => false;

    // The pre-Tuned mechanic (starting a combat already carrying Tuned 1) — same shape as
    // IsPrePlanned above, for "big one-off moment" B cards that should already be primed to lock
    // after their first play rather than needing a card-side grant.
    public virtual bool IsPreTuned => false;

    // True once this card has actually been pre-Tuned for the current combat — reset only
    // at BeforeCombatStart, same reasoning as _prePlannedThisCombat (AfterCardEnteredCombat fires
    // on every pile transition, not just the first).
    private bool _preTunedThisCombat;

    private void ApplyPreTunedIfNeeded()
    {
        if (!IsPreTuned || _preTunedThisCombat) return;
        if (Pile?.Type.IsCombatPile() != true) return;
        if (this.TryGetModifier<TunedModifier>(out _)) return;

        _preTunedThisCombat = true;
        TunedModifier.Apply(this);
    }

    public override Task BeforeCombatStart()
    {
        var t = base.BeforeCombatStart();
        _preTunedThisCombat = false;
        // Reset (not just lazily set) here: a previous combat's snapshot/StableModifier grant
        // must not leak into a new one — MaybeSnapshotIfStable only ever sets _stableSnapshot
        // once it's null, so without this reset a printed-Stable card would only ever get
        // snapshotted on its very first combat, never refreshed on later ones.
        _stableSnapshot = null;
        // Apply pre-Tuned BEFORE the first Stable snapshot so a card that is BOTH Stable and pre-Tuned
        // freezes WITH its Tuned stack. Otherwise the snapshot (taken without Tuned) would treat the
        // pre-Tuned modifier as foreign and strip it on the next restore. No card is currently both
        // (Practice was, until Stable was removed from it), and this is a no-op for the non-Stable
        // pre-Tuned cards — but the ordering is free and keeps the combination safe if one is made.
        ApplyPreTunedIfNeeded();
        EnforceStableNow();
        return t;
    }

    // The two counter powers are granted lazily, and only once the player actually HAS a Planned /
    // Tuned card — a fresh deck with neither mechanic in it should not carry two permanently-empty
    // counters. The gate has to be on the grant rather than on the powers' IsVisible: NPowerContainer
    // .Add builds a power's icon node only if IsVisible at the instant PowerApplied fires and never
    // re-checks it, so a power granted while hidden could never appear later. Gating the grant also
    // latches for free — nothing removes these powers mid-combat, so the counter stays put once shown
    // rather than blinking out when the last Tuned card is exhausted.
    //
    // Called from every hook that can follow a Planned/Tuned application: turn start (pre-Planned /
    // pre-Tuned setup, Rosin), card play (the bulk of both mechanics, plus AutoTune/Muse/Perfectionism
    // powers, which run their AfterCardPlayed before ours — powers are iterated before cards), and
    // potion use (Planned/Tuned Potion).
    private static async Task GrantCountersIfNeeded(PlayerChoiceContext context, Player player)
    {
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower)
            && PlannedModifier.AnyIn(PlannedModifier.RelevantCards(player)))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
        if (!player.Creature.Powers.Any(p => p is TunedCounterPower)
            && TunedModifier.TunedCards(player).Any())
            await PowerCmd.Apply<TunedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    // Auto-attach the shared counter powers so the queue/Tuned UI badges appear as soon as either
    // mechanic is in play, and the hidden InvertTrackerPower so Invert can react to
    // enemy-inflicted (not just self-applied) invertible debuffs and perform its bidirectional
    // debuff/buff cancellation for all 6 pairs (see InvertTrackerPower for why that logic lives
    // there rather than on each Un-X power). (Take Notes' "debuff cleared" detection used to need a
    // similar hidden tracker here, but now lives in DebuffClearOnRemovePatch — see DebuffClearNotifier.)
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        EnforceStableNow();
        if (player != Owner) return;
        // Once per combat: give every pre-Planned card a concrete, unique Planned slot in deck order.
        // Driven here (rather than per-card at BeforeCombatStart) because by turn 1's start every combat
        // card is present in the draw/hand piles, so the ordered pass sees them all.
        PrePlannedSetup.AssignIfNeeded(player, CombatState!);
        await GrantCountersIfNeeded(context, player);
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
        // Pre-Tuned BEFORE the snapshot, for the same reason BeforeCombatStart does it in that order:
        // if the snapshot is taken first it won't contain Tuned, and StableEnforcementPatch will then
        // strip the pre-Tuned modifier the instant it is added as a foreign type. This path can run
        // before this card's own BeforeCombatStart — a relic that adds cards during the
        // BeforeCombatStart pass (Tea of Discourtesy) fires AfterCardEnteredCombat on every card
        // mid-pass — so it cannot rely on that method having already taken the snapshot WITH Tuned.
        ApplyPreTunedIfNeeded();
        EnforceStableNow();
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        EnforceStableNow();

        // Most Planned/Tuned applications happen during a card play, so this is where the counter
        // powers usually first appear. Owner-gated so one card does the grant, not every card in hand.
        if (Owner != null && cardPlay.Card.Owner == Owner)
            await GrantCountersIfNeeded(context, Owner);

        // Planned is only ever removed by an explicit "remove Planned" effect or by a "Play all
        // Planned" resolver (Showtime/DaCapo/Workshop/Remix) consuming the exact slot it's
        // resolving. A card that's simply playable (Unplayable freed some other way, e.g. by
        // Unwind/Confidence/StartOver) just plays normally when clicked manually — its own Planned
        // slot(s) are untouched and it stays queued to auto-play later too.
        //
        // The Tuned->Unplayable lock used to live here, but only fired for cards deriving from
        // UnderstudyCard (colorless Tuned cards escaped it). It now lives in TunedLockPower, a hidden
        // player power that observes every card play (see AfterPlayerTurnStartLate).
    }

    // The Planned/Tuned Potions apply their modifier outside any card play, so the counter grant needs
    // its own trigger here or the badge would not show until the next card played. This hook carries no
    // PlayerChoiceContext; applying a power asks the player nothing, so a ThrowingPlayerChoiceContext is
    // safe (same pattern as SafetyNet and DebuffClearNotifier).
    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (Owner == null || CombatState == null) return;
        await GrantCountersIfNeeded(new ThrowingPlayerChoiceContext(), Owner);
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        EnforceStableNow();
        return Task.CompletedTask;
    }

    // The single Stable-enforcement entry point, called at every significant combat event (combat
    // start, card entered/played, both sides' turn start, side turn end) plus — for immediate reaction
    // to modifications from any source — this card's own KeywordsChanged/ReplayCountChanged events and
    // the ApplyInternal Harmony patch (StableEnforcementPatch). The first time the card is observed
    // Stable it takes the deep snapshot (whether printed, or granted mid-combat by e.g. Final Draft on another card's OnPlay)
    // and starts watching keyword edits; every call then reconciles the live card back to that frozen
    // config via StableEnforcer.Restore. Deep restore undoes in-place mutation (e.g. an emptied Planned
    // slot list), so a Stable Planned card keeps its Planned through a queue resolution.
    private void EnforceStableNow()
    {
        if (_stableSnapshot == null)
        {
            if (!this.IsStable()) return;
            _stableSnapshot = StableEnforcer.Capture(this);
            if (!_stableWatch)
            {
                KeywordsChanged += OnStableDrift;
                ReplayCountChanged += OnStableDrift;
                _stableWatch = true;
            }
        }
        if (StableEnforcer.Restore(this, _stableSnapshot))
            RefreshStablePlannedVisuals();
    }

    // Fires the instant this card's local keywords are edited (e.g. base-game Ethereal from Hex/Music Box)
    // or its replay count is written (Master Form's Replay grant) — revert immediately rather than waiting
    // for the next combat-event hook, which for Replay would be after the extra play already happened.
    // Guarded against re-entry from Restore's own AddKeyword/RemoveKeyword and BaseReplayCount write.
    private void OnStableDrift()
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

    // Stop watching for drift and drop the snapshot so nothing leaks into the next combat.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_stableWatch)
        {
            KeywordsChanged -= OnStableDrift;
            ReplayCountChanged -= OnStableDrift;
            _stableWatch = false;
        }
        _stableSnapshot = null;
        return Task.CompletedTask;
    }
}
