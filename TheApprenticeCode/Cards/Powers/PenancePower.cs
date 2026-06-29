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
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class PenancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Penance",
        "Whenever you lose [gold]Strength[/gold], gain {Amount} [gold]Block[/gold].",
        "Whenever you lose [gold]Strength[/gold], gain {Amount} [gold]Block[/gold].");

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext ctx, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not StrengthPower || power.Owner != Owner || amount >= 0m) return;
        int lostStrength = (int)Math.Abs(amount);
        await CreatureCmd.GainBlock(Owner, lostStrength * Amount, ValueProp.Unpowered, null);
    }
}
