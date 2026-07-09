using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Pure presence flag, same shape as HeldNotePower.IsActive — checked by the three call sites that
// add UnplayableModifier as a result of a card carrying IntenseModifier (UnderstudyCard.AfterCardPlayed,
// PlannedModifier.Apply, ShakenPower.BeforeSideTurnEnd), so an Intense card can be played, Planned,
// or Shaken without locking up while this Power is active.
public class MuscleMemoryPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Muscle Memory",
        "[gold]Intense[/gold] cards will not become [gold]Unplayable[/gold].",
        "[gold]Intense[/gold] cards will not become [gold]Unplayable[/gold].");

    public static bool IsActive(Creature? creature) => creature?.GetPower<MuscleMemoryPower>() != null;
}
