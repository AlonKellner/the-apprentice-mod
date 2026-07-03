using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class TheFirstLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "The First Lesson",
        "You cannot become [gold]Weak[/gold] or [gold]Vulnerable[/gold].",
        "You cannot become [gold]Weak[/gold] or [gold]Vulnerable[/gold].");

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;
        if (canonicalPower is not (WeakPower or VulnerablePower)) return false;
        modifiedAmount = 0m;
        return true;
    }
}
