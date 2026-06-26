using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class WunderkindPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Wunderkind",
        "At the start of your turn, add 1 [gold]Dream+[/gold] and 1 [gold]Ambition+[/gold] to your hand.",
        "At the start of your turn, add 1 [gold]Dream+[/gold] and 1 [gold]Ambition+[/gold] to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;
        await DreamsAndAmbitions.AddDreams(player, Owner.CombatState!, 1, upgraded: true);
        await DreamsAndAmbitions.AddAmbitions(player, Owner.CombatState!, 1, upgraded: true);
    }
}
