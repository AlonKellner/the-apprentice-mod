using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class MasterFormPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Master Form",
        "When attacks or skills without [gold]Replay[/gold] become [gold]Unplayable[/gold], they gain [gold]Replay[/gold].",
        "When attacks or skills without [gold]Replay[/gold] become [gold]Unplayable[/gold], they gain [gold]Replay[/gold] [blue]{Amount}[/blue].");

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied += OnUnplayableApplied;
        return Task.CompletedTask;
    }

    // NOT AfterRemoved: Creature.RemoveAllPowersInternalExcept (the bulk wipe used at combat end)
    // is explicitly documented to skip the AfterRemoved hook for every power it clears, so it never
    // fires on a normal combat end. AfterCombatEnd is the hook PlannedCounterPower already
    // established as the reliable one for this exact cleanup shape. Without this, the static
    // subscription lingers into every future combat, granting Replay even with Master Form nowhere
    // in play.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
        return Task.CompletedTask;
    }

    private void OnUnplayableApplied(CardModel card)
    {
        if (card.Owner?.Creature != Owner) return;
        if (card.BaseReplayCount == 0) card.BaseReplayCount = Amount;
    }
}
