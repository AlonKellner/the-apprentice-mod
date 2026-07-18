using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Recurring passive Swap: every turn start, push your invertible debuffs onto enemies. Amount is the
// per-turn Swap count (2), fixed — the upgrade adds Innate, not more Swap.
public class StagePresencePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Stage Presence",
        "At the start of your turn, [gold]Swap[/gold] this many.",
        "At the start of your turn, [gold]Swap[/gold] {Amount}.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        await SceneStealing.SwapEach(context, Owner, (int)Amount);
    }
}
