using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnweakPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Hidden while empty — always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unweak",
        "Unweakened creatures deal [blue]25%[/blue] more damage with Attacks. Cancels out with Weak.",
        "Unweakened creatures deal [blue]25%[/blue] more damage with Attacks. Cancels out with Weak.");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 1.25m;
    }

    private int _pendingWeakConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against WeakPower: this branch (existing direction) reduces an
    // incoming Weak gain by my own current stock. The second branch (new direction) reduces an
    // incoming gain to MYSELF by however much Weak currently exists — this is what lets Invert's
    // conversion (and each Fortissimo repeat of it) re-check the live Weak stock at the moment
    // each individual gain lands, rather than netting everything once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is WeakPower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingWeakConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curWeak = Owner.GetPowerAmount<WeakPower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curWeak);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is WeakPower && _pendingWeakConsumption > 0)
        {
            for (int i = 0; i < _pendingWeakConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingWeakConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var weak = Owner.GetPower<WeakPower>();
            if (weak == null)
            {
                Log.Error("UnweakPower consumed Weak stock via interception but GetPower<WeakPower> " +
                          "now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(weak);
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
