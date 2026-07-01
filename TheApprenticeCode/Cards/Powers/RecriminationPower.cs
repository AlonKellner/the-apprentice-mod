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

public class RecriminationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Recrimination",
        "At the start of your turn, apply your [gold]Vulnerable[/gold] and [gold]Weak[/gold] stacks to ALL enemies.",
        "At the start of your turn, apply your [gold]Vulnerable[/gold] and [gold]Weak[/gold] stacks to ALL enemies.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;

        int vulnAmount = Owner.GetPowerAmount<VulnerablePower>();
        if (vulnAmount > 0)
            await PowerCmd.Apply<VulnerablePower>(context, Owner.CombatState!.HittableEnemies, vulnAmount, Owner, null, false);

        int weakAmount = Owner.GetPowerAmount<WeakPower>();
        if (weakAmount > 0)
            await PowerCmd.Apply<WeakPower>(context, Owner.CombatState!.HittableEnemies, weakAmount, Owner, null, false);
    }
}
