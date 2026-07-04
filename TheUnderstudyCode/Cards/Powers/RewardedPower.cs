using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Granted 1 stack whenever an Order is obeyed (The Second Lesson). Never decreases or is removed
// on its own — a plain data-holder Counter power with no overridden hooks. Every turn,
// SecondLessonPower applies Amount stacks of one priority-then-randomly-selected buff and consumes
// nothing here; the logic is centralized on SecondLessonPower (see its own comments) so
// Reward-before-Punish ordering doesn't depend on cross-power hook iteration order.
public class RewardedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Rewarded",
        "Every turn, apply this many of a random [gold]invertible[/gold] buff to yourself.",
        "Every turn, apply {Amount} of a random [gold]invertible[/gold] buff to yourself.");
}
