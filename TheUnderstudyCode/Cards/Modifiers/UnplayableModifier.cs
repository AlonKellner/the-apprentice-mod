using System;
using System.Collections.Generic;
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
    // skills").
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && card.IsUnplayable();

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
