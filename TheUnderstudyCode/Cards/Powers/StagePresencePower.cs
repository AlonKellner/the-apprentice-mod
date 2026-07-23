using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Recurring passive Swap: every turn start, trade fortunes with the enemy team once. Amount is the
// per-turn Swap repeat count (1), fixed — the upgrade adds Innate, not more Swap.
public class StagePresencePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;


    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        await SceneStealing.Swap(context, Owner, (int)Amount);
    }
}
