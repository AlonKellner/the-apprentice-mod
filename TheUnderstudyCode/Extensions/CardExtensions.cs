using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

public static class CardExtensions
{
    // Reasons CanPlay() reports that should NOT count as "functionally unplayable" here: every
    // hand has cards you simply haven't paid for yet, and counting those would make Understudy's
    // Unplayable-payoff cards swing on leftover energy instead of an actual affliction/debuff.
    private const UnplayableReason ExcludedReasons =
        UnplayableReason.EnergyCostTooHigh | UnplayableReason.StarCostTooHigh;

    // Pure predicate over CanPlay()'s reason flags — split out from IsUnplayable so the
    // cost-exclusion logic is testable without a live Card/CombatState.
    public static bool IsFunctionallyUnplayableReason(UnplayableReason reason) =>
        (reason & ~ExcludedReasons) != UnplayableReason.None;

    public static bool IsUnplayable(this CardModel card)
    {
        // Native + modifier-added Unplayable keyword. Checked manually (rather than solely via
        // CanPlay() below) because it also needs to work without a live CombatState — bare-
        // instantiated cards in tests, or any other out-of-combat context.
        if (card.Keywords.Contains(CardKeyword.Unplayable))
            return true;
        var kw = new HashSet<CardKeyword>();
        foreach (var mod in CardModifier.DirectModifiers(card))
            mod.TryModifyKeywordsInCombat(card, kw);
        if (kw.Contains(CardKeyword.Unplayable))
            return true;

        // Broader "functionally unplayable" reasons — a hook blocking it (e.g. base-game Smog),
        // the card's own built-in play condition, or no valid ally target — the same "crossed
        // out" look the player reads as "this card is stuck." Only assessable with a live
        // CombatState; CanPlay() is a graceful no-op (returns false with reason None) without one.
        if (!card.CanPlay(out var reason, out _))
            return IsFunctionallyUnplayableReason(reason);
        return false;
    }
}
