using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class FortissimoPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Fortissimo",
        "All invertible buff and debuff gains are doubled.",
        "All invertible buff and debuff gains are doubled.");

    private static bool IsInvertiblePower(PowerModel power) =>
        power is WeakPower or UnweakPower
            or VulnerablePower or UnvulnerablePower
            or ShakenPower or UnshakenPower
            or LimitedPower or UnlimitedPower
            or JadedPower or UnjadedPower
            or StrengthPower or DexterityPower;

    public override decimal ModifyPowerAmountGivenMultiplicative(
        PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (giver != Owner || amount <= 0m || !IsInvertiblePower(power)) return 1m;
        return 2m;
    }
}
