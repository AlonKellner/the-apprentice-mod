using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class PlannedModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Planned";

    public int SequenceIndex { get; set; }

    // Adds the Unplayable keyword dynamically (global layer) so it wears off
    // automatically when this modifier is removed. If TryModifyKeywordsInCombat
    // is not forwarded to CardModifiers at runtime, add PlannedPatches.cs as fallback.
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner)
        {
            keywords.Add(ApprenticeKeywords.Planned);
            keywords.Add(CardKeyword.Unplayable);
            return true;
        }
        return false;
    }

    public override void ModifyDescriptionPost(Creature? creature, ref string description)
    {
        description += $"\n[gold]Planned[/gold] #{SequenceIndex + 1}.";
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.IntProperties["seq"] = SequenceIndex;
    }

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("seq", out int seq))
            SequenceIndex = seq;
    }
}
