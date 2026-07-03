using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class MasterFormPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Master Form",
        "Whenever you apply [gold]Planned[/gold] or [gold]Intense[/gold] to an attack or skill that doesn't have Replay, it gains Replay 1.",
        "Whenever you apply [gold]Planned[/gold] or [gold]Intense[/gold] to an attack or skill that doesn't have Replay, it gains Replay 1.");

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        PlannedModifier.Applied += OnApplied;
        IntenseModifier.Applied += OnApplied;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        PlannedModifier.Applied -= OnApplied;
        IntenseModifier.Applied -= OnApplied;
        return Task.CompletedTask;
    }

    private void OnApplied(CardModel card)
    {
        if (card.Owner?.Creature != Owner) return;
        if (card.BaseReplayCount == 0) card.BaseReplayCount = 1;
    }
}
