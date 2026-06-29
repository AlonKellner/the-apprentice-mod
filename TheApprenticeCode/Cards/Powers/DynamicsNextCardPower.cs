using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// Short-lived marker. TensionHelper.AddTension reads Amount as extra Tension bonus,
// then removes this power after the next Tension gain.
public class DynamicsNextCardPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Dynamics",
        "The next card you play gains {Amount} additional [gold]Tension[/gold].",
        "The next card you play gains {Amount} additional [gold]Tension[/gold].");
}
