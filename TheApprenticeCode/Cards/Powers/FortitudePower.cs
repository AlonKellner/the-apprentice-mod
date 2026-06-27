using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class FortitudePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Fortitude",
        "At the start of your turn, if you are [gold]Weak[/gold], gain 1 [gold]Strength[/gold].",
        "At the start of your turn, if you are [gold]Weak[/gold] or [gold]Vulnerable[/gold], gain 1 [gold]Strength[/gold].");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;

        bool hasWeak = Owner.GetPowerAmount<WeakPower>() > 0;
        bool hasVulnerable = Amount >= 2 && Owner.GetPowerAmount<VulnerablePower>() > 0;

        if (hasWeak || hasVulnerable)
            await PowerCmd.Apply<StrengthPower>(context, Owner, 1m, Owner, null, false);
    }
}
