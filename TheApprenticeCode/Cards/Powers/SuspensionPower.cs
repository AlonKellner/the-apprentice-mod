using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// Amount is set by the Suspension card (5 base, 8 upgraded).
// Each turn start: gain Amount Tension and Amount Vigor.
public class SuspensionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Suspension",
        "At the start of your turn, gain {Amount} [gold]Tension[/gold] and {Amount} [gold]Vigor[/gold].",
        "At the start of your turn, gain {Amount} [gold]Tension[/gold] and {Amount} [gold]Vigor[/gold].");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;
        await TensionHelper.AddTension(context, Owner, (int)Amount, null);
        await PowerCmd.Apply<VigorPower>(context, Owner, Amount, Owner, null, false);
    }
}
