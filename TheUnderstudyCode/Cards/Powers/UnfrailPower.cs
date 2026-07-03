using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnfrailPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unfrail",
        "Unfrail creatures gain [blue]25%[/blue] more Block. Cancels out with Frail.",
        "Unfrail creatures gain [blue]25%[/blue] more Block. Cancels out with Frail.");

    // Hidden while empty — this power is now always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; it only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0) return 1m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 1m;
        return 1.25m;
    }

    private int _pendingFrailConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against FrailPower: this branch (existing direction) reduces an
    // incoming Frail gain by my own current stock. The second branch (new direction) reduces an
    // incoming gain to MYSELF by however much Frail currently exists — this is what lets Invert's
    // conversion (and each Fortissimo repeat of it) re-check the live Frail stock at the moment
    // each individual gain lands, rather than netting everything once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is FrailPower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingFrailConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curFrail = Owner.GetPowerAmount<FrailPower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curFrail);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is FrailPower && _pendingFrailConsumption > 0)
        {
            for (int i = 0; i < _pendingFrailConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingFrailConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var frail = Owner.GetPower<FrailPower>();
            if (frail == null)
            {
                Log.Error("UnfrailPower consumed Frail stock via interception but GetPower<FrailPower> " +
                          "now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(frail);
            }
            _pendingSelfConsumption = 0;
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && Amount > 0 && !HeldNotePower.IsActive(Owner))
            await PowerCmd.TickDownDuration(this);
    }
}
