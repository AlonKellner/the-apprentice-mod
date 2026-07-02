using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class UnplayableModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Unplayable";

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner) { keywords.Add(CardKeyword.Unplayable); return true; }
        return false;
    }
}
