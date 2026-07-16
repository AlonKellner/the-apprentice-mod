using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// Runtime-grantable counterpart to the printed-only Stable keyword (WithKeyword in a card's own
// constructor, used by Workshop/Practice). Lets a card effect (e.g. Final Draft) make an
// arbitrary Attack/Skill Stable mid-combat instead of only at print-time.
public class StableModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Stable";

    // Attacks and Skills only, not already Stable (printed or via this modifier) — matches the
    // eligibility shape of PlannedModifier/TunedModifier/UnplayableModifier.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && !card.IsStable();

    public static void Apply(CardModel card)
    {
        if (!card.TryGetModifier<StableModifier>(out _))
            CardModifier.AddModifier<StableModifier>(card);
    }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner)
        {
            keywords.Add(UnderstudyKeywords.Stable);
            return true;
        }
        return false;
    }
}
