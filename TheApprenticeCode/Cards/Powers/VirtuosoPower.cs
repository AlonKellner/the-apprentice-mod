using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class VirtuosoPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Virtuoso",
        "[gold]Planned[/gold] cards lose the [gold]Unplayable[/gold] keyword.",
        "");

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card.TryGetModifier<PlannedModifier>(out _))
        {
            keywords.Remove(CardKeyword.Unplayable);
            return true;
        }
        return false;
    }
}
