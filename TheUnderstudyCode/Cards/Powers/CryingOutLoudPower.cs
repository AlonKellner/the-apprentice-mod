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
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class CryingOutLoudPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        DebuffClearNotifier.DebuffCleared += OnDebuffCleared;
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
        DebuffClearNotifier.DebuffCleared -= OnDebuffCleared;
        return Task.CompletedTask;
    }

    private async Task OnDebuffCleared(PlayerChoiceContext ctx, Creature creature, PowerModel power)
    {
        if (creature != Owner) return;
        await PowerCmd.Apply<VigorPower>(ctx, Owner, Amount, Owner, null, false);
    }

    // The amount-change half of "a debuff of yours clears": a negative Buff (Vigor / Strength / Dexterity,
    // which act as debuffs while below 0) lifted back to >= 0 — spending negative Vigor on an attack, or
    // Invert/Swap/decay raising a negative stat. Debuff-typed clears instead come through
    // DebuffClearNotifier (removal); EmotionalExpression.IsDebuffClear is false for a Debuff type here, so
    // this fires only on the negative-Buff crossing — the same shared IsDebuff definition, no double count.
    // The old amount is recoverable because `amount` is the change delta (old = new - delta).
    private bool _granting;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_granting || power.Owner != Owner) return;
        if (!EmotionalExpression.IsDebuffClear(power.Type, power.Amount - amount, power.Amount)) return;

        // Re-entrancy guard: the Vigor we grant could itself lift a separate negative Vigor across 0 and
        // re-enter here. One external clear should grant exactly once.
        _granting = true;
        try { await PowerCmd.Apply<VigorPower>(context, Owner, Amount, Owner, null, false); }
        finally { _granting = false; }
    }
}
