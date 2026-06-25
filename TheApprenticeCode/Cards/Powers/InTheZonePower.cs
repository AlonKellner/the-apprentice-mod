using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class InTheZonePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "In the Zone",
        "At the start of your turn, if you have any [gold]Planned[/gold] cards, gain {Amount} Energy.",
        "");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        bool hasPlanned = PlannedModifier.AnyIn(player.Piles.SelectMany(p => p.Cards));
        if (hasPlanned)
            await PlayerCmd.GainEnergy(Amount, player);
    }
}
