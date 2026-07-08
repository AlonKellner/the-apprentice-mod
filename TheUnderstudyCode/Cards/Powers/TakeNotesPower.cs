using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class TakeNotesPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Take Notes",
        "Whenever a debuff of yours clears, gain {Amount} Vigor.",
        "Whenever a debuff of yours clears, gain {Amount} Vigor.");

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        DebuffClearTrackerPower.DebuffCleared += OnDebuffCleared;
        return Task.CompletedTask;
    }

    // NOT AfterRemoved: Creature.RemoveAllPowersInternalExcept (the bulk wipe used at combat end)
    // is explicitly documented to skip the AfterRemoved hook for every power it clears, so that
    // hook never fires on a normal combat end — only on an explicit mid-combat removal, which
    // nothing currently does to this power. AfterCombatEnd is the hook PlannedCounterPower already
    // established as the reliable one for this exact cleanup shape (see its own AfterCombatEnd).
    // Without this, the static subscription lingers into every future combat, granting Vigor on
    // any debuff clearing even with Take Notes nowhere in play.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        DebuffClearTrackerPower.DebuffCleared -= OnDebuffCleared;
        return Task.CompletedTask;
    }

    private async Task OnDebuffCleared(PlayerChoiceContext ctx, Creature creature, PowerModel power)
    {
        if (creature != Owner) return;
        await PowerCmd.Apply<VigorPower>(ctx, Owner, Amount, Owner, null, false);
    }
}
