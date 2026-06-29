using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class ExpendModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Expend";

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner) { keywords.Add(ApprenticeKeywords.Expend); return true; }
        return false;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == Owner)
            CardModifier.AddModifier<SpentModifier>(cardPlay.Card);
        await Task.CompletedTask;
    }
}
