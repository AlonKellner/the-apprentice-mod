using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "The roles of invertible buffs and debuffs are reversed." Purely a flag power — all the actual
// swap logic lives on InvertTrackerPower (see its "reversed mode" branch), checked there via
// Owner.GetPowerAmount<MyOwnLessonPower>() > 0. Adding a second TryModifyPowerAmountReceived listener
// here would race InvertTrackerPower's own interception for the same event with no reliable way to
// control which runs first (see InvertTrackerPower.cs for the full reasoning) — so this class
// deliberately carries no interception of its own.
public class MyOwnLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "My Own Lesson",
        "Whenever you gain an [gold]Invertible[/gold] debuff, gain a buff instead. Whenever you gain an [gold]Invertible[/gold] buff, gain nothing instead.",
        "Whenever you gain an [gold]Invertible[/gold] debuff, gain a buff instead. Whenever you gain an [gold]Invertible[/gold] buff, gain nothing instead.");
}
