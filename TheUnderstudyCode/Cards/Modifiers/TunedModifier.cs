using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class TunedModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Tuned";

    // How many times Tuned has been applied to this card (its "level").
    public int Stacks { get; private set; }

    // Set only when a card's FIRST-EVER Tuned stack is granted "after the check" — i.e. after
    // that same play's own attack/block calculation (ModifyDamageAdditive/ModifyBlockAdditive)
    // already ran, so the newly granted stack had no effect on this play (Da Capo: it grants
    // itself Tuned only after its own attack resolves). Stores the exact CardPlay this happened
    // during, compared by reference in IsFinalTunedPlay below, so it only suppresses locking for
    // THIS specific play — the very next time the card is played, it already carries Tuned
    // before that play's own check runs, so the normal rule (lock on the play after Tuned is
    // already active) applies again.
    private CardPlay? _grantedAfterOwnCheckDuringPlay;

    // True exactly once per real card play: on the last CardPlay in a Replay series
    // (PlayIndex == PlayCount - 1; a card with no Replay has PlayCount = 1, so its single
    // play already satisfies this). Cards without TunedModifier never qualify, and a card whose
    // only Tuned stack was granted after its own check this same play (see
    // _grantedAfterOwnCheckDuringPlay above) doesn't qualify for THIS play either. Used by
    // UnderstudyCard.AfterCardPlayed to decide when to attach UnplayableModifier, and by
    // BenchedPower to know when a Tuned card has finished being played.
    public static bool IsFinalTunedPlay(CardPlay cardPlay) =>
        cardPlay.IsLastInSeries
        && cardPlay.Card.TryGetModifier<TunedModifier>(out var mod)
        && !ReferenceEquals(mod!._grantedAfterOwnCheckDuringPlay, cardPlay);

    // ── Tuned card count ─────────────────────────────────────────────────────────────────────
    // This card's full Tuned damage/block bonus: its stack count times the number of cards carrying
    // Tuned (Strength/Dexterity-style flat additive). Single source of truth, reused by both
    // ModifyBase*Additive below and the card-preview patch (which shows the bonus on cards viewed in
    // the draw/discard pile, where the game doesn't run the damage/block hooks).
    public int Bonus => Stacks * TunedCardCount();

    // A live count, exactly as the keyword text promises ("for each card with Tuned") — deliberately
    // not a tally of Apply calls. A card can come to carry Tuned without ever passing through Apply:
    // the base game's Music Box clones a played card wholesale, modifiers and all, so cloning a
    // pre-Tuned card (Practice) mints a second Tuned carrier behind Apply's back. Counting live means
    // such a card contributes, and there is no counter left that can drift out of step with reality.
    //
    // Scoped to the combat piles — Draw/Hand/Discard/Exhaust/Play, so an exhausted Tuned card still
    // counts — and deliberately not the Deck pile, whose cards are the out-of-combat master copies
    // that combat modifications never reach.
    //
    // virtual so tests can supply a count directly: the bare test host cannot stand up a combat, and
    // an unattached modifier legitimately counts zero.
    protected virtual int TunedCardCount()
    {
        // CardModel.Owner asserts mutability, so a canonical/bare card must not be asked for it.
        if (Owner is not { IsMutable: true } card) return 0;
        var player = card.Owner;
        if (player == null) return 0;
        return player.Piles.Where(p => p.Type.IsCombatPile()).SelectMany(p => p.Cards)
            .Count(c => c.TryGetModifier<TunedModifier>(out _));
    }

    // The live Tuned carriers in this player's combat piles — the same set TunedCardCount() sizes (so its
    // Count() is the current per-stack Tuned bonus, the +damage/+block each Tuned card adds). Exposed for
    // the Tuned counter power's card-name list; order is not meaningful.
    public static IEnumerable<CardModel> TunedCards(Player? player) =>
        player == null
            ? Enumerable.Empty<CardModel>()
            : player.Piles.Where(p => p.Type.IsCombatPile()).SelectMany(p => p.Cards)
                .Where(c => c.TryGetModifier<TunedModifier>(out _));

    // Re-entrancy guard for the Spectacle-target question below — the exact analogue of
    // PlannedModifier.QueueNeedsEnemyTarget's _evaluatingQueueTarget. Spectacle can itself be Tuned, so it
    // appears in TunedCards; reading its TargetType while answering "does any Tuned card want an enemy?"
    // would re-enter this method forever (a stack-overflow crash) without the flag.
    [ThreadStatic] private static bool _evaluatingTunedTarget;

    // Whether any Tuned card in the player's combat piles wants an enemy target — Spectacle.TargetType uses
    // this instead of scanning c.TargetType inline, so a Tuned Spectacle can't recurse into its own getter.
    public static bool TunedQueueNeedsEnemyTarget(Player? player)
    {
        if (_evaluatingTunedTarget) return false;
        _evaluatingTunedTarget = true;
        try { return TunedCards(player).Any(c => c.TargetType == TargetType.AnyEnemy); }
        finally { _evaluatingTunedTarget = false; }
    }

    // Any non-Stable Attack/Skill is eligible — matches PlannedModifier/UnplayableModifier's own
    // eligibility check. A card with no Damage/Block var (e.g. Workshop-shaped utility Skills)
    // still becomes Unplayable when played; it just gets zero numeric bonus from
    // ModifyDamageAdditive/ModifyBlockAdditive below, since those already no-op via their own
    // props.IsPoweredAttack()/IsPoweredCardOrMonsterMoveBlock() checks.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && !card.IsStable();

    // Raised the first time a card receives Tuned (not on subsequent re-Tensifies of the
    // same card) — Master Form's "whenever you apply... Tuned... that doesn't have Replay" trigger.
    public static event Action<CardModel>? Applied;

    // grantedAfterOwnCheck: pass the current CardPlay when this call happens after that same
    // card's own attack/block for THIS play has already been calculated — i.e. the card is
    // granting Tuned to itself, too late for it to have counted this play (Da Capo is currently
    // the only card that does this). Leave null for the normal case of applying Tuned to a
    // different card (Innovation/Rehearse/Practice), where timing-within-this-play is
    // irrelevant since that other card isn't the one currently resolving.
    public static void Apply(CardModel card, CardPlay? grantedAfterOwnCheck = null)
    {
        bool firstApplication = !card.TryGetModifier<TunedModifier>(out var mod);
        if (firstApplication)
        {
            CardModifier.AddModifier<TunedModifier>(card);
            // The addition is not guaranteed to stick: a Stable card whose frozen snapshot predates
            // Tuned has StableEnforcementPatch strip it back off as a foreign type the instant it
            // lands, leaving nothing to stack onto. Assuming otherwise crashed combat start outright
            // (NullReferenceException out of Tea of Discourtesy generating cards mid-BeforeCombatStart).
            // Bail quietly instead — the card simply does not become Tuned.
            if (!card.TryGetModifier<TunedModifier>(out mod)) return;
            Applied?.Invoke(card);
        }

        mod!.Stacks++;
        if (firstApplication && grantedAfterOwnCheck != null)
            mod._grantedAfterOwnCheckDuringPlay = grantedAfterOwnCheck;
        // No explicit BaseValue update needed — ModifyDamageAdditive / ModifyBlockAdditive
        // inject the bonus at calculation time (same mechanism as Strength / Dexterity).
        // Nothing to keep in step either: TunedCardCount reads the live carriers each time.
    }

    // Doubles an already-Tuned card's Stacks in place (Cut the Tension: Tuned 1 -> 2, Tuned
    // 3 -> 6). No-op on a card with no TunedModifier, and it adds no new Tuned carrier, so the
    // card count is unaffected — this scales one card's own stacks, not how many cards are Tuned.
    // Stacks keeps its private setter; mutated only through this class's own static API, matching
    // Apply's existing convention.
    public static void DoubleStacks(CardModel card)
    {
        if (card.TryGetModifier<TunedModifier>(out var mod))
            mod!.Stacks *= 2;
    }

    // ── Strength/Dexterity-style bonus ─────────────────────────────────────────────────────────
    // DAMAGE is delivered through BaseLib's card-modifier contract: BaseLib's ModifyBaseDamagePatches
    // Harmony patch on Hook.ModifyDamage invokes ModifyBaseDamageAdditive directly on the calculated
    // card's own modifiers (cardSource.GetModifiers()). We deliberately do NOT override the game's
    // 5/6-arg AbstractModel.ModifyDamageAdditive: it only reaches a card modifier through the game's
    // hook-listener enumeration, whose signature and run-state routing drift between game versions.
    //
    // BLOCK is delivered by our own ModifyBaseBlockPatch, which reads Bonus off this modifier DIRECTLY
    // (see that class) rather than through BaseLib's CardModifier.ModifyBaseBlockAdditive virtual.
    // BaseLib never bridged that block virtual and, as of 3.4.x, marked it [Obsolete("Not currently
    // functional")] AND changed its signature (added a ValueProp param) — a mod compiled against the
    // old one-arg signature threw MissingMethodException at runtime against the newer BaseLib. Reading
    // Bonus keeps Tuned's block bonus off that drifting/deprecated API entirely.
    //
    // originalDamage is unused — Tuned is a flat additive, like Strength/Dexterity. The powered-attack
    // gate lives here (props is available); the powered-block gate lives in ModifyBaseBlockPatch.

    public override decimal ModifyBaseDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack()) return 0m;
        return Bonus;
    }

    // ── Instance overrides ───────────────────────────────────────────────────────────────────

    // The explanation a Tuned card carries, phrased for its actual stack count. The keyword's own
    // card_keywords entry hardcodes "by 1 for each card", which understates a multi-stack card
    // (Experience doubles stacks), so this restates both clauses with the real number.
    public static string TipDescription(int stacks) =>
        "Becomes [gold]Unplayable[/gold] when played. Increases damage or [gold]Block[/gold] by "
        + $"{stacks} for each card with [gold]Tuned[/gold].";

    // Every card that HAS Tuned explains Tuned, whether or not its printed text mentions the keyword.
    // BaseLib routes CardModel.HoverTips through ExtraTooltips.AddTips, which calls this on each of the
    // card's modifiers — so this reaches colorless and base-game cards too, which a per-card WithTip in
    // an UnderstudyCard constructor never could. (TryModifyKeywordsInCombat below deliberately does not
    // add the keyword, so the game's own keyword->tip loop never fires for Tuned; this is the only
    // source.) Cards whose text mentions Tuned still declare the static WithTip for
    // CardTooltipKeywordSyncTests — drop it here so the stack-aware wording wins instead of doubling up.
    public override void AddTips(List<IHoverTip> tips)
    {
        if (Stacks <= 0) return;
        string keywordTipId = HoverTipFactory.FromKeyword(UnderstudyKeywords.Tuned).Id;
        tips.RemoveAll(t => t.Id == keywordTipId);
        // Carry the Tuned counter power's icon so this stack-aware tip matches the plain Tuned keyword tip
        // (which gets the same icon via CounterKeywordIconPatch) and the Planned tooltip. Re-resolved per
        // call (PowerModel.Icon is a cache-reuse load), so it's never a disposed cross-combat texture.
        tips.Add(new HoverTip(
            new LocString("card_keywords", "THEUNDERSTUDY-TUNED.title"),
            TipDescription(Stacks),
            ModelDb.Power<TunedCounterPower>().Icon));
    }

    // UnderstudyKeywords.Tuned is NOT added here — that would create a second un-numbered
    // "Tuned" badge alongside ModifyDescription's "Tuned N." text.
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return false;
    }

    // Prepended BEFORE the card description (e.g. "Tuned 2.\nDeal N damage.")
    // so the stack count appears above the main card text, matching the user's
    // "Tuned should be before description" requirement.
    public override void ModifyDescription(Creature? creature, ref string description)
    {
        if (Stacks <= 0) return;
        description = $"[gold]Tuned {Stacks}[/gold].\n" + description;
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.IntProperties["stacks"] = Stacks;
    }

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("stacks", out int s)) Stacks = s;
    }
}
