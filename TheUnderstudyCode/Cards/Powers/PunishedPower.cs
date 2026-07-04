using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Granted 1 stack whenever an Order is disobeyed (The Second Lesson). Never decreases or is
// removed on its own — a plain data-holder Counter power with no overridden hooks; see
// RewardedPower's own comment for why the actual per-turn application lives centrally on
// SecondLessonPower instead of here.
public class PunishedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Punished",
        "Every turn, apply this many of a random [gold]invertible[/gold] debuff to yourself.",
        "Every turn, apply {Amount} of a random [gold]invertible[/gold] debuff to yourself.");
}
