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

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 1m;
        return 1.25m;
    }

    private int _pendingConsumption;

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        if (target != Owner || canonicalPower is not FrailPower || amount <= 0)
        {
            modifiedAmount = amount;
            return false;
        }
        var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
        _pendingConsumption = consumed;
        modifiedAmount = reduced;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is not FrailPower || _pendingConsumption <= 0) return;
        for (int i = 0; i < _pendingConsumption; i++)
            await PowerCmd.Decrement(this);
        _pendingConsumption = 0;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && !HeldNotePower.IsActive(Owner))
            await PowerCmd.TickDownDuration(this);
    }
}
