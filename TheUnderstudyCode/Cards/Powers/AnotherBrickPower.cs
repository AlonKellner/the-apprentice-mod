using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class AnotherBrickPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    // Broader than the "last modified invertible debuff" tracker in EmotionalExpression — this fires for
    // ANY debuff gain landing on you, self- or enemy-inflicted, invertible or not. Via IsDebuffApplication
    // that also includes a buff driven negative (e.g. Muffle spending Vigor below 0 = a self-debuff).
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || !EmotionalExpression.IsDebuffApplication(power, amount)) return;
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, false);
    }
}
