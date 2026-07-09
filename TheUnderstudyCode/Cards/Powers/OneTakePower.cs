using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class OneTakePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "One Take",
        "Decrease all card costs by 1. Apply [gold]Unplayable[/gold] to every played card.",
        "Decrease all card costs by {Amount}. Apply [gold]Unplayable[/gold] to every played card.");

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner) return false;
        modifiedCost = originalCost - Amount;
        return true;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner && !cardPlay.Card.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(cardPlay.Card);
        return Task.CompletedTask;
    }
}
