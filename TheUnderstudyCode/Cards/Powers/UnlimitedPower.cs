using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnlimitedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Hidden while empty — always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unlimited",
        "At the start of your next turn, draw until your hand is full.",
        "At the start of your next turn, draw until your hand is full.");

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player || Amount <= 0) return count;
        // CardPileCmd.Draw already clamps its own draw loop to CardPile.MaxCardsInHand,
        // stopping early once the hand is full — requesting the cap as the draw count is
        // always enough to reach it (or drain the deck trying) without a manual hand-size read.
        return Math.Max(count, CardPile.MaxCardsInHand);
    }

    private int _pendingLimitedConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against LimitedPower: this branch reduces an incoming Limited
    // gain by my own current stock. The second branch reduces an incoming gain to MYSELF by
    // however much Limited currently exists — this is what lets Invert's conversion (and each
    // Fortissimo repeat of it) re-check the live Limited stock at the moment each individual gain
    // lands, rather than netting everything once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is LimitedPower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingLimitedConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curLimited = Owner.GetPowerAmount<LimitedPower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curLimited);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is LimitedPower && _pendingLimitedConsumption > 0)
        {
            for (int i = 0; i < _pendingLimitedConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingLimitedConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var limited = Owner.GetPower<LimitedPower>();
            if (limited == null)
            {
                Log.Error("UnlimitedPower consumed Limited stock via interception but " +
                          "GetPower<LimitedPower> now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(limited);
            }
            _pendingSelfConsumption = 0;
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Amount <= 0) return;
        Flash();
        if (!HeldNotePower.IsActive(Owner))
            await PowerCmd.Decrement(this);
    }
}
