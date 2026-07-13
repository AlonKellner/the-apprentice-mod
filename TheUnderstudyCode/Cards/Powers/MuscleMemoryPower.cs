using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "Tense cards will not become Unplayable." Immunity is NOT enforced here per call site — it's
// enforced centrally in UnplayableModifier.OnInitialApplication (the one point every Unplayable
// attach funnels through), which checks IsActive below. This Power only supplies that presence flag.
public class MuscleMemoryPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Muscle Memory",
        "[gold]Tense[/gold] cards will not become [gold]Unplayable[/gold].",
        "[gold]Tense[/gold] cards will not become [gold]Unplayable[/gold].");

    public static bool IsActive(Creature? creature) => creature?.GetPower<MuscleMemoryPower>() != null;
}
