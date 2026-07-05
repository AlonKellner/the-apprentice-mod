using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class IntenseModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Intense";

    // How many times Intense has been applied to this card (its "level").
    public int Stacks { get; private set; }

    // Set only when a card's FIRST-EVER Intense stack is granted "after the check" — i.e. after
    // that same play's own attack/block calculation (ModifyDamageAdditive/ModifyBlockAdditive)
    // already ran, so the newly granted stack had no effect on this play (Da Capo: it grants
    // itself Intense only after its own attack resolves). Stores the exact CardPlay this happened
    // during, compared by reference in IsFinalIntensePlay below, so it only suppresses locking for
    // THIS specific play — the very next time the card is played, it already carries Intense
    // before that play's own check runs, so the normal rule (lock on the play after Intense is
    // already active) applies again.
    private CardPlay? _grantedAfterOwnCheckDuringPlay;

    // True exactly once per real card play: on the last CardPlay in a Replay series
    // (PlayIndex == PlayCount - 1; a card with no Replay has PlayCount = 1, so its single
    // play already satisfies this). Cards without IntenseModifier never qualify, and a card whose
    // only Intense stack was granted after its own check this same play (see
    // _grantedAfterOwnCheckDuringPlay above) doesn't qualify for THIS play either. Used by
    // UnderstudyCard.AfterCardPlayed to decide when to attach UnplayableModifier, and by
    // BenchedPower to know when an Intense card has finished being played.
    public static bool IsFinalIntensePlay(CardPlay cardPlay) =>
        cardPlay.IsLastInSeries
        && cardPlay.Card.TryGetModifier<IntenseModifier>(out var mod)
        && !ReferenceEquals(mod!._grantedAfterOwnCheckDuringPlay, cardPlay);

    // ── Combat-scoped counter ────────────────────────────────────────────────────────────────
    // Counts distinct cards that have received at least one Intense application this combat.
    // Applying Intense 3× to the same card still counts as 1 distinct card.
    // Bonus per card = Stacks × _intenseCreated (read at damage/block calculation time).
    private static ICombatState? _lastCombat;
    private static int _intenseCreated;

    private static void MaybeResetForCombat(ICombatState combat)
    {
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _intenseCreated = 0;
        }
    }

    // A card can receive Intense only if it has at least one stat that the Intense bonus
    // can actually modify via the powered hook — i.e. a DamageVar or BlockVar with
    // ValueProp.Move (the flag set by WithDamage/WithBlock, which satisfies IsPoweredAttack
    // and IsPoweredCardOrMonsterMoveBlock). Cards with no Damage/Block var (e.g. Performance,
    // Intention) or with unpowered props are excluded.
    public static bool CanApplyTo(CardModel card)
    {
        if (card.Keywords.Contains(UnderstudyKeywords.Stable)) return false;
        bool hasPoweredDamage = card.DynamicVars.TryGetValue("Damage", out var dmgVar)
            && ((DamageVar)dmgVar).Props.IsPoweredAttack();
        bool hasPoweredBlock = card.DynamicVars.TryGetValue("Block", out var blkVar)
            && ((BlockVar)blkVar).Props.IsPoweredCardOrMonsterMoveBlock();
        return hasPoweredDamage || hasPoweredBlock;
    }

    // Raised the first time a card receives Intense (not on subsequent re-Intensifies of the
    // same card) — Master Form's "whenever you apply... Intense... that doesn't have Replay" trigger.
    public static event Action<CardModel>? Applied;

    // grantedAfterOwnCheck: pass the current CardPlay when this call happens after that same
    // card's own attack/block for THIS play has already been calculated — i.e. the card is
    // granting Intense to itself, too late for it to have counted this play (Da Capo is currently
    // the only card that does this). Leave null for the normal case of applying Intense to a
    // different card (CramSession/Rehearse/TouchUp/Intention), where timing-within-this-play is
    // irrelevant since that other card isn't the one currently resolving.
    public static void Apply(CardModel card, ICombatState combat, IEnumerable<CardModel> allCards, CardPlay? grantedAfterOwnCheck = null)
    {
        MaybeResetForCombat(combat);

        bool firstApplication = !card.TryGetModifier<IntenseModifier>(out var mod);
        if (firstApplication)
        {
            // First application to this card: count it as a new Intense card.
            CardModifier.AddModifier<IntenseModifier>(card);
            card.TryGetModifier<IntenseModifier>(out mod);
            _intenseCreated++;
            Applied?.Invoke(card);
        }

        mod!.Stacks++;
        if (firstApplication && grantedAfterOwnCheck != null)
            mod._grantedAfterOwnCheckDuringPlay = grantedAfterOwnCheck;
        // No explicit BaseValue update needed — ModifyDamageAdditive / ModifyBlockAdditive
        // inject the bonus at calculation time (same mechanism as Strength / Dexterity).

        Invariants.CheckEqual(_intenseCreated, allCards.Count(c => c.TryGetModifier<IntenseModifier>(out _)),
            nameof(IntenseModifier) + "." + nameof(Apply),
            "distinct cards carrying IntenseModifier this combat vs. _intenseCreated");
    }

    // ── Strength/Dexterity-style hook overrides ──────────────────────────────────────────────
    // These are called by Hook.ModifyDamage / Hook.ModifyBlock at card preview and play time,
    // causing the card to display and deal/grant the bonus automatically, with green/red coloring
    // when the final value differs from the base value (same as how Strength renders on cards).

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != Owner) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        Invariants.Check(ReferenceEquals(_lastCombat, cardSource?.CombatState),
            nameof(IntenseModifier) + "." + nameof(ModifyDamageAdditive),
            "_intenseCreated may be stale — this ran before any IntenseModifier.Apply call this combat");
        return Stacks * _intenseCreated;
    }

    public override decimal ModifyBlockAdditive(
        Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != Owner) return 0m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 0m;
        Invariants.Check(ReferenceEquals(_lastCombat, cardSource?.CombatState),
            nameof(IntenseModifier) + "." + nameof(ModifyBlockAdditive),
            "_intenseCreated may be stale — this ran before any IntenseModifier.Apply call this combat");
        return Stacks * _intenseCreated;
    }

    // ── Instance overrides ───────────────────────────────────────────────────────────────────

    // UnderstudyKeywords.Intense is NOT added here — that would create a second un-numbered
    // "Intense" badge alongside ModifyDescription's "Intense N." text.
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return false;
    }

    // Prepended BEFORE the card description (e.g. "Intense 2.\nDeal N damage.")
    // so the stack count appears above the main card text, matching the user's
    // "Intense should be before description" requirement.
    public override void ModifyDescription(Creature? creature, ref string description)
    {
        if (Stacks <= 0) return;
        description = $"[gold]Intense {Stacks}[/gold].\n" + description;
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
