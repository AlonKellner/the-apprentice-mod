using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class AmbitousModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Ambitous";

    public int OriginalBaseDamage { get; set; }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner) { keywords.Add(ApprenticeKeywords.Ambitous); return true; }
        return false;
    }

    public override void StoreSaveData(ModifierSave save) =>
        save.IntProperties["origDmg"] = OriginalBaseDamage;

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("origDmg", out int v))
            OriginalBaseDamage = v;
    }
}
