using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class TenseModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Tense";

    // How many times Tense has been applied to this card (its "level").
    public int Stacks { get; private set; }

    // Set only when a card's FIRST-EVER Tense stack is granted "after the check" — i.e. after
    // that same play's own attack/block calculation (ModifyDamageAdditive/ModifyBlockAdditive)
    // already ran, so the newly granted stack had no effect on this play (Da Capo: it grants
    // itself Tense only after its own attack resolves). Stores the exact CardPlay this happened
    // during, compared by reference in IsFinalTensePlay below, so it only suppresses locking for
    // THIS specific play — the very next time the card is played, it already carries Tense
    // before that play's own check runs, so the normal rule (lock on the play after Tense is
    // already active) applies again.
    private CardPlay? _grantedAfterOwnCheckDuringPlay;

    // True exactly once per real card play: on the last CardPlay in a Replay series
    // (PlayIndex == PlayCount - 1; a card with no Replay has PlayCount = 1, so its single
    // play already satisfies this). Cards without TenseModifier never qualify, and a card whose
    // only Tense stack was granted after its own check this same play (see
    // _grantedAfterOwnCheckDuringPlay above) doesn't qualify for THIS play either. Used by
    // UnderstudyCard.AfterCardPlayed to decide when to attach UnplayableModifier, and by
    // BenchedPower to know when a Tense card has finished being played.
    public static bool IsFinalTensePlay(CardPlay cardPlay) =>
        cardPlay.IsLastInSeries
        && cardPlay.Card.TryGetModifier<TenseModifier>(out var mod)
        && !ReferenceEquals(mod!._grantedAfterOwnCheckDuringPlay, cardPlay);

    // ── Combat-scoped counter ────────────────────────────────────────────────────────────────
    // Counts distinct cards that have received at least one Tense application this combat.
    // Applying Tense 3× to the same card still counts as 1 distinct card.
    // Bonus per card = Stacks × _tenseCreated (read at damage/block calculation time).
    private static ICombatState? _lastCombat;
    private static int _tenseCreated;

    private static void MaybeResetForCombat(ICombatState combat)
    {
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _tenseCreated = 0;
        }
    }

    // Any non-Stable Attack/Skill is eligible — matches PlannedModifier/UnplayableModifier's own
    // eligibility check. A card with no Damage/Block var (e.g. Performance-shaped utility Skills)
    // still becomes Unplayable when played; it just gets zero numeric bonus from
    // ModifyDamageAdditive/ModifyBlockAdditive below, since those already no-op via their own
    // props.IsPoweredAttack()/IsPoweredCardOrMonsterMoveBlock() checks.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && !card.IsStable();

    // Raised the first time a card receives Tense (not on subsequent re-Tensifies of the
    // same card) — Master Form's "whenever you apply... Tense... that doesn't have Replay" trigger.
    public static event Action<CardModel>? Applied;

    // grantedAfterOwnCheck: pass the current CardPlay when this call happens after that same
    // card's own attack/block for THIS play has already been calculated — i.e. the card is
    // granting Tense to itself, too late for it to have counted this play (Da Capo is currently
    // the only card that does this). Leave null for the normal case of applying Tense to a
    // different card (CramSession/Rehearse/TouchUp/Buildup), where timing-within-this-play is
    // irrelevant since that other card isn't the one currently resolving.
    public static void Apply(CardModel card, ICombatState combat, IEnumerable<CardModel> allCards, CardPlay? grantedAfterOwnCheck = null)
    {
        MaybeResetForCombat(combat);

        bool firstApplication = !card.TryGetModifier<TenseModifier>(out var mod);
        if (firstApplication)
        {
            // First application to this card: count it as a new Tense card.
            CardModifier.AddModifier<TenseModifier>(card);
            card.TryGetModifier<TenseModifier>(out mod);
            _tenseCreated++;
            Applied?.Invoke(card);
        }

        mod!.Stacks++;
        if (firstApplication && grantedAfterOwnCheck != null)
            mod._grantedAfterOwnCheckDuringPlay = grantedAfterOwnCheck;
        // No explicit BaseValue update needed — ModifyDamageAdditive / ModifyBlockAdditive
        // inject the bonus at calculation time (same mechanism as Strength / Dexterity).

        Invariants.CheckEqual(_tenseCreated, allCards.Count(c => c.TryGetModifier<TenseModifier>(out _)),
            nameof(TenseModifier) + "." + nameof(Apply),
            "distinct cards carrying TenseModifier this combat vs. _tenseCreated");
    }

    // Doubles an already-Tense card's Stacks in place (Cut the Tension: Tense 1 -> 2, Tense
    // 3 -> 6). No-op on a card with no TenseModifier. _tenseCreated is untouched — it counts
    // distinct cards, not stacks. Stacks keeps its private setter; mutated only through this
    // class's own static API, matching Apply's existing convention.
    public static void DoubleStacks(CardModel card)
    {
        if (card.TryGetModifier<TenseModifier>(out var mod))
            mod!.Stacks *= 2;
    }

    // ── Strength/Dexterity-style bonus, delivered via BaseLib's card-modifier contract ─────────
    // These are the ModifyBase* hooks BaseLib invokes directly on the calculated card's own
    // modifiers (cardSource.GetModifiers()): the damage side via BaseLib's ModifyBaseDamagePatches
    // Harmony patch on Hook.ModifyDamage, the block side via our own ModifyBaseBlockPatch (BaseLib
    // ships no block equivalent — see that class). We deliberately do NOT override the game's 5/6-arg
    // AbstractModel.ModifyDamageAdditive / ModifyBlockAdditive: those only reach a card modifier
    // through the game's hook-listener enumeration, whose signature and (for damage) run-state
    // routing drift between game versions — a Slay the Spire 2 update that added a CardPlay? param
    // to ModifyDamageAdditive silently unbound that override and killed Tense's damage bonus for
    // players on the newer build while Block (whose signature was unchanged) kept working. The
    // ModifyBase* signatures are BaseLib-owned and stable across those game changes.
    //
    // BaseLib calls these only on THIS card's own modifiers, so no cardSource/Owner check is needed.
    // originalDamage/originalBlock are unused — Tense is a flat additive, like Strength/Dexterity.
    // The powered-attack gate lives here (props is available); the powered-block gate lives in
    // ModifyBaseBlockPatch, because BaseLib's ModifyBaseBlockAdditive virtual carries no props.

    public override decimal ModifyBaseDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack()) return 0m;
        return Stacks * _tenseCreated;
    }

    public override decimal ModifyBaseBlockAdditive(decimal originalBlock)
    {
        return Stacks * _tenseCreated;
    }

    // ── Instance overrides ───────────────────────────────────────────────────────────────────

    // UnderstudyKeywords.Tense is NOT added here — that would create a second un-numbered
    // "Tense" badge alongside ModifyDescription's "Tense N." text.
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return false;
    }

    // Prepended BEFORE the card description (e.g. "Tense 2.\nDeal N damage.")
    // so the stack count appears above the main card text, matching the user's
    // "Tense should be before description" requirement.
    public override void ModifyDescription(Creature? creature, ref string description)
    {
        if (Stacks <= 0) return;
        description = $"[gold]Tense {Stacks}[/gold].\n" + description;
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
