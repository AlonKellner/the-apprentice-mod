using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class BrioPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Brio",
        "Whenever a debuff is applied to you during your turn, gain {Amount} [gold]Vigor[/gold].",
        "Whenever a debuff is applied to you during your turn, gain {Amount} [gold]Vigor[/gold].");

    private bool _isPlayerTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature == Owner) _isPlayerTurn = true;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side == CombatSide.Player) _isPlayerTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext ctx, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!_isPlayerTurn || power.Owner != Owner || power.Type != PowerType.Debuff || amount <= 0m) return;
        await PowerCmd.Apply<VigorPower>(ctx, Owner, Amount, Owner, cardSource, false);
    }
}
