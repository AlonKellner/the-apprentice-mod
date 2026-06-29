using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

// Marker power. While active, TensionPower skips end-of-turn damage and carries Tension over.
// TensionPower removes this power after making the carry-over decision.
public class DeceptiveCadencePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Deceptive Cadence",
        "Tension will not damage you this turn. Tension is NOT removed at end of turn.",
        "Tension will not damage you this turn. Tension is NOT removed at end of turn.");
}
