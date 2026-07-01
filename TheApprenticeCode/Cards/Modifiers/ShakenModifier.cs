using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class ShakenModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Shaken";

    public bool IsActive { get; private set; } = true;

    public void Activate() => IsActive = true;
    public void Reset() => IsActive = false;

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card != Owner || !IsActive) return false;
        keywords.Add(CardKeyword.Unplayable);
        return true;
    }
}
