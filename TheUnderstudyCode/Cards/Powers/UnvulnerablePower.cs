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

public class UnvulnerablePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Hidden while empty — always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unvulnerable",
        "Unvulnerable creatures take [blue]25%[/blue] less damage from Attacks. Cancels out with Vulnerable.",
        "Unvulnerable creatures take [blue]25%[/blue] less damage from Attacks. Cancels out with Vulnerable.");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 0.75m;
    }

    private int _pendingVulnerableConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against VulnerablePower: this branch (existing direction) reduces
    // an incoming Vulnerable gain by my own current stock. The second branch (new direction)
    // reduces an incoming gain to MYSELF by however much Vulnerable currently exists — this is
    // what lets Invert's conversion (and each Fortissimo repeat of it) re-check the live
    // Vulnerable stock at the moment each individual gain lands, rather than netting everything
    // once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is VulnerablePower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingVulnerableConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curVulnerable = Owner.GetPowerAmount<VulnerablePower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curVulnerable);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is VulnerablePower && _pendingVulnerableConsumption > 0)
        {
            for (int i = 0; i < _pendingVulnerableConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingVulnerableConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var vulnerable = Owner.GetPower<VulnerablePower>();
            if (vulnerable == null)
            {
                Log.Error("UnvulnerablePower consumed Vulnerable stock via interception but " +
                          "GetPower<VulnerablePower> now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(vulnerable);
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
