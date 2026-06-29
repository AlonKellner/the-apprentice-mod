using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class FanfarePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Fanfare",
        "The first time each debuff type is applied to you in combat, gain 1 Energy.",
        "The first time each debuff type is applied to you in combat, gain 1 Energy.");

    private readonly HashSet<Type> _triggeredDebuffs = new();

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext ctx, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || power.Type != PowerType.Debuff || amount <= 0m) return;
        if (_triggeredDebuffs.Add(power.GetType()))
            await PlayerCmd.GainEnergy(1, Owner.Player!);
    }
}
