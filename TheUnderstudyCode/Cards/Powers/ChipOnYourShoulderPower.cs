using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class ChipOnYourShoulderPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Chip on Your Shoulder",
        "Whenever a debuff is applied to you during your turn, gain {Amount} [gold]Vigor[/gold].",
        "Whenever a debuff is applied to you during your turn, gain {Amount} [gold]Vigor[/gold].");

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext ctx, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || Owner.CombatState?.CurrentSide != Owner.Side) return;
        if (power.GetTypeForAmount(amount) != PowerType.Debuff || amount <= 0m) return;
        await PowerCmd.Apply<VigorPower>(ctx, Owner, Amount, Owner, cardSource, false);
    }
}
