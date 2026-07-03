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

public class FullVoicePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Full Voice",
        "Whenever a debuff is applied to you, gain {Amount} Block.",
        "Whenever a debuff is applied to you, gain {Amount} Block.");

    // Broader than the "last modified invertible debuff" tracker in EmotionalExpression — this
    // fires for ANY debuff gain, self- or enemy-inflicted, invertible or not.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || power.Type != PowerType.Debuff || amount <= 0m) return;
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, false);
    }
}
