using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class UnplayableModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Unplayable";

    // Only Attacks and Skills can ever become Unplayable via Planned/Intense, and "Remove
    // Unplayable" only ever targets those, so a card is only a valid target once it's actually
    // Unplayable (matches the wording used everywhere: "Remove Unplayable from N attacks and
    // skills"). CardExtensions.IsUnplayable() covers "functionally unplayable" broadly (Smog-style
    // hook blocks, etc.), not just this modifier's own keyword — Remove() below only clears our
    // own flag, so selecting a card made Unplayable some other way is a no-op for that card, same
    // as it already was for natively Unplayable-keyworded cards before this was broadened.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && card.IsUnplayable();

    // Whether any card in the given set is a valid "remove Unplayable" target — used by
    // TakeTwo/Rewrite/TouchUp to glow gold only when they'd actually have something to free.
    public static bool AnyIn(IEnumerable<CardModel> cards) => cards.Any(CanApplyTo);

    // Removes the standalone Unplayable flag. Does not touch PlannedModifier/IntenseModifier —
    // a card can still be Planned/Intense afterward, it just stops being forced Unplayable by
    // this specific modifier (Planned re-adds it the next time Apply is called).
    public static void Remove(CardModel card)
    {
        if (card.TryGetModifier<UnplayableModifier>(out var mod))
            CardModifier.DirectModifiers(card).Remove(mod);
    }

    // Raised whenever this modifier is newly attached to a card (Standing By's "whenever a card
    // becomes Unplayable" trigger).
    public static event Action<CardModel>? Applied;

    public override void OnInitialApplication()
    {
        if (Owner != null) Applied?.Invoke(Owner);
    }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner) { keywords.Add(CardKeyword.Unplayable); return true; }
        return false;
    }
}
