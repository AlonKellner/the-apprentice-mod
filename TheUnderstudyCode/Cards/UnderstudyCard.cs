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
    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    // Call in subclass constructors to register the dynamic Tense hover tip. The lambda is
    // evaluated at hover time with the live card, so it reads the current TenseModifier.Stacks —
    // returns nothing when Tense hasn't been applied yet (no tip clutter on fresh cards).
    protected void WithTenseTip()
    {
        WithTips(card =>
        {
            if (!card.TryGetModifier<TenseModifier>(out var mod) || mod.Stacks <= 0)
                return Enumerable.Empty<IHoverTip>();
            int s = mod.Stacks;
            return new IHoverTip[]
            {
                new HoverTip(
                    new LocString("card_keywords", "THEUNDERSTUDY-TENSE.title"),
                    $"If this deals damage or grants [gold]Block[/gold], increase it by {s} for each card with [gold]Tense[/gold]."
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

    // Snapshot of DirectModifiers at combat start; null means this card is not Stable.
    private List<CardModifier>? _stableSnapshot;

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
    // silently re-queue it — self-perpetuating, unlike Refrain's genuinely deliberate self-Plan.
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

    // The pre-Tense mechanic (starting a combat already carrying Tense 1) — same shape as
    // IsPrePlanned above, for "big one-off moment" B cards that should already be primed to lock
    // after their first play rather than needing a card-side grant.
    public virtual bool IsPreTense => false;

    // True once this card has actually been pre-Tensified for the current combat — reset only
    // at BeforeCombatStart, same reasoning as _prePlannedThisCombat (AfterCardEnteredCombat fires
    // on every pile transition, not just the first).
    private bool _preTenseThisCombat;

    private void ApplyPreTenseIfNeeded()
    {
        if (!IsPreTense || _preTenseThisCombat) return;
        if (Pile?.Type.IsCombatPile() != true) return;
        if (this.TryGetModifier<TenseModifier>(out _)) return;

        _preTenseThisCombat = true;
        TenseModifier.Apply(this, CombatState!, Owner!.Piles.SelectMany(p => p.Cards));
    }

    public override Task BeforeCombatStart()
    {
        var t = base.BeforeCombatStart();
        _prePlannedThisCombat = false;
        _preTenseThisCombat = false;
        // Reset (not just lazily set) here: a previous combat's snapshot/StableModifier grant
        // must not leak into a new one — MaybeSnapshotIfStable only ever sets _stableSnapshot
        // once it's null, so without this reset a printed-Stable card would only ever get
        // snapshotted on its very first combat, never refreshed on later ones.
        _stableSnapshot = null;
        MaybeSnapshotIfStable();
        ApplyPrePlannedIfNeeded();
        ApplyPreTenseIfNeeded();
        return t;
    }

    // Auto-attach the shared PlannedCounterPower so the queue UI badge is visible whenever
    // Performance queues cards, the hidden InvertTrackerPower so Invert can react to
    // enemy-inflicted (not just self-applied) invertible debuffs and perform its bidirectional
    // debuff/buff cancellation for all 6 pairs (see InvertTrackerPower for why that logic lives
    // there rather than on each Un-X power), and the hidden DebuffClearTrackerPower so Take Notes
    // (and anything future) can react to any debuff clearing for any reason, not just this deck's
    // own Invert conversions.
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        MaybeSnapshotIfStable();
        RestoreIfStable();
        if (player != Owner) return;
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
        if (!player.Creature.Powers.Any(p => p is InvertTrackerPower))
            await PowerCmd.Apply<InvertTrackerPower>(context, player.Creature, 1m, player.Creature, null, false);
        if (!player.Creature.Powers.Any(p => p is DebuffClearTrackerPower))
            await PowerCmd.Apply<DebuffClearTrackerPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    // Restore on every card-play and turn boundary so no window exists where a Stable card
    // appears modified — covers enemy-applied effects (Ethereal, etc.) that slip past the
    // CanApplyTo guards.
    public override Task AfterCardEnteredCombat(CardModel triggeredBy)
    {
        MaybeSnapshotIfStable();
        RestoreIfStable();
        ApplyPrePlannedIfNeeded();
        ApplyPreTenseIfNeeded();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        MaybeSnapshotIfStable();
        RestoreIfStable();

        // Planned is only ever removed by an explicit "remove Planned" effect or by a "Play all
        // Planned" resolver (CurtainCall/Encore/Performance/Remix) consuming the exact slot it's
        // resolving. A card that's simply playable (Unplayable freed some other way, e.g. by
        // TakeTwo/SafetyNet/MissedCue) just plays normally when clicked manually — its own Planned
        // slot(s) are untouched and it stays queued to auto-play later too. Tense is the only
        // keyword that changes a card as a result of being played (below).

        // Safe to mutate DirectModifiers here: AfterCardPlayed fires once BaseLib's per-modifier
        // OnPlay enumeration (BeforeAfterPlayHooks) has finished for this play, unlike a
        // CardModifier's own OnPlay override, which runs inside that enumeration and throws
        // "Collection was modified" if it tries to add a modifier to the same card.
        // IsFinalTensePlay gates this to the last CardPlay in a Replay series, so a card with
        // Replay N only becomes Unplayable after all N+1 plays have resolved. Muscle Memory
        // suppresses this specifically (not Planned-driven Unplayable in general) — see
        // MuscleMemoryPower's own doc comment for the other two call sites this mirrors.
        if (cardPlay.Card == this
            && TenseModifier.IsFinalTensePlay(cardPlay)
            && !this.TryGetModifier<UnplayableModifier>(out _)
            && !MuscleMemoryPower.IsActive(Owner?.Creature))
            CardModifier.AddModifier<UnplayableModifier>(this);
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        MaybeSnapshotIfStable();
        RestoreIfStable();
        return Task.CompletedTask;
    }

    // Takes the protective snapshot the first time this card is observed to be Stable — whether
    // printed (BeforeCombatStart already sees it) or granted mid-combat by an effect like Final
    // Draft (StableModifier), which attaches on a completely different card's OnPlay and so isn't
    // itself a hook firing on THIS card. Called right before RestoreIfStable() everywhere the
    // latter already runs, so a runtime-granted Stable starts being protected within one hook
    // boundary of being granted, without needing a new event subscription (this.IsStable() is a
    // plain per-instance check, not a static subscription that could leak past combat end).
    private void MaybeSnapshotIfStable()
    {
        if (_stableSnapshot == null && this.IsStable())
            _stableSnapshot = CardModifier.DirectModifiers(this).ToList();
    }

    private void RestoreIfStable()
    {
        if (_stableSnapshot == null) return;
        var mods = CardModifier.DirectModifiers(this);
        var toRemove = mods.Where(m => !_stableSnapshot.Contains(m)).ToList();
        foreach (var m in toRemove) mods.Remove(m);
        var toRestore = _stableSnapshot.Where(m => !mods.Contains(m)).ToList();
        foreach (var m in toRestore) mods.Add(m);
    }
}
