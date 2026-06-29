using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// Amount=1: double Tension gains. Amount>=2 (upgraded card): triple Tension gains.
// TensionHelper reads this power to apply the correct multiplier.
public class CadencePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization
    {
        get
        {
            string desc = Amount >= 2
                ? "All [gold]Tension[/gold] you gain is tripled."
                : "All [gold]Tension[/gold] you gain is doubled.";
            return new PowerLoc("Cadence", desc, desc);
        }
    }
}
