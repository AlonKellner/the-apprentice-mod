using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class BrightSidePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        // Living the Dream: when Stage Presence is also present, it runs the combined simultaneous Best of
        // Both, so skip the separate Invert (otherwise it would fire in addition to the combined resolution).
        if (Owner.GetPowerAmount<StagePresencePower>() > 0) return;
        await EmotionalExpression.InvertEach(context, Owner, Amount);
    }

    // Keep the visible Living the Dream marker in sync whenever any power on the owner changes.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner == Owner) await LivingTheDreamPower.Sync(context, Owner);
    }
}
