using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class AdLibPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Ad Lib",
        "At the start of your turn, Invert 1 random [gold]invertible[/gold] debuff you currently have.",
        "At the start of your turn, Invert {Amount} random [gold]invertible[/gold] debuff you currently have.");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;

        var present = EmotionalExpression.GetPresentInvertibleDebuffs(Owner);
        if (present.Count == 0) return;

        var chosen = player.RunState.Rng.CombatCardSelection.NextItem(present);
        await EmotionalExpression.InvertDebuff(context, Owner, chosen, Amount);
    }
}
