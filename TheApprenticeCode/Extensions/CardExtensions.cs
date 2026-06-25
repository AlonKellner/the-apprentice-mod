using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Extensions;

public static class CardExtensions
{
    public static bool IsUnplayable(this CardModel card)
    {
        // Native keywords (Injury, curses, status cards, other-character Unplayable cards)
        if (card.Keywords.Contains(CardKeyword.Unplayable))
            return true;

        // Modifier-added keywords (runtime effects other than PlannedModifier)
        var kw = new HashSet<CardKeyword>();
        foreach (var mod in CardModifier.DirectModifiers(card))
            mod.TryModifyKeywordsInCombat(card, kw);
        return kw.Contains(CardKeyword.Unplayable);
    }
}
