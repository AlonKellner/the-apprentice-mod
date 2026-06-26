using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class ProdigyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Prodigy",
        "When you play a [gold]Dream[/gold], add 1 [gold]Ambition[/gold] to your hand. When you play an [gold]Ambition[/gold], add 1 [gold]Dream[/gold] to your hand.",
        "When you play a [gold]Dream[/gold], add 1 [gold]Ambition[/gold] to your hand. When you play an [gold]Ambition[/gold], add 1 [gold]Dream[/gold] to your hand. [gold]Innate[/gold].");

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var player = cardPlay.Card.Owner;

        if (cardPlay.Card is Dream)
            await DreamsAndAmbitions.AddAmbitions(player, Owner.CombatState!, 1);
        else if (cardPlay.Card is Ambition)
            await DreamsAndAmbitions.AddDreams(player, Owner.CombatState!, 1);
    }
}
