using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class ConductorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Conductor",
        "At the start of your turn, add 1 [gold]Potential[/gold] to your hand.",
        "At the start of your turn, add 1 [gold]Potential+[/gold] to your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;
        bool upgraded = Amount >= 2;
        await DreamsAndAmbitions.AddPotentials(player, Owner.CombatState!, 1, upgraded);
    }
}
