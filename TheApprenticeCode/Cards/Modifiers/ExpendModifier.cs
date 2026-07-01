using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class ExpendModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Expend";

    public bool IsSpent { get; private set; }

    public void Reset()
    {
        IsSpent = false;
        if (Owner?.TryGetModifier<UnplayableModifier>(out var u) == true)
            CardModifier.DirectModifiers(Owner).Remove(u);
    }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card != Owner) return false;
        keywords.Add(ApprenticeKeywords.Expend);
        return true;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == Owner)
        {
            IsSpent = true;
            if (!Owner!.TryGetModifier<UnplayableModifier>(out _))
                CardModifier.AddModifier<UnplayableModifier>(Owner!);
        }
        await Task.CompletedTask;
    }

    public override void StoreSaveData(ModifierSave save) =>
        save.IntProperties["spent"] = IsSpent ? 1 : 0;

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("spent", out int v))
            IsSpent = v != 0;
    }
}
