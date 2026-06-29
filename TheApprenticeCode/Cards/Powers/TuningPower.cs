using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// TensionHelper.AddTension reads Amount as a flat bonus added to every Tension gain.
public class TuningPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Tuning",
        "Whenever you gain [gold]Tension[/gold], gain {Amount} additional [gold]Tension[/gold].",
        "Whenever you gain [gold]Tension[/gold], gain {Amount} additional [gold]Tension[/gold].");
}
