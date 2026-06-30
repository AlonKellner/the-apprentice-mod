using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// Amount=1: TensionPower redirects end-of-turn damage to all enemies instead of self.
// Amount>=2 (upgraded card): deals that damage twice (see TensionPower.BeforeSideTurnEnd).
public class FortissimoPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization
    {
        get
        {
            string desc = Amount >= 2
                ? "[gold]Tension[/gold] no longer damages you. At the end of your turn, deal your [gold]Tension[/gold] as damage to ALL enemies twice, then remove all [gold]Tension[/gold]."
                : "[gold]Tension[/gold] no longer damages you. At the end of your turn, deal your [gold]Tension[/gold] as damage to ALL enemies, then remove all [gold]Tension[/gold].";
            return new PowerLoc("Fortissimo", desc, desc);
        }
    }
}
