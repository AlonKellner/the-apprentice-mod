using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class DreamyModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Dreamy";

    public int OriginalBaseBlock { get; set; }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner) { keywords.Add(ApprenticeKeywords.Dreamy); return true; }
        return false;
    }

    public override void StoreSaveData(ModifierSave save) =>
        save.IntProperties["origBlock"] = OriginalBaseBlock;

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("origBlock", out int v))
            OriginalBaseBlock = v;
    }
}
