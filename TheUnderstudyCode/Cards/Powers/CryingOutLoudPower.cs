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

    // Negative Vigor is a functional debuff — Vigor is Buff-typed but AllowNegative (VigorAllowNegativePatch)
    // lets it drop below 0, reducing your attacks. DebuffClearNotifier can't see it clear: it only fires for
    // Type==Debuff powers, and by removal time (Vigor is removed at exactly 0) the amount is already 0, so
    // the sign is gone. Catch it here instead — when this creature's Vigor rises from below 0 to >= 0
    // (spending it on an attack zeroes it; Invert/Swap/Silence can also lift it), a debuff cleared, so grant
    // Vigor the same as any other clear. `amount` is the change delta, so old = new - delta. The grant runs
    // from Vigor >= 0 (old >= 0 there), so it can't re-satisfy the condition — no re-entrancy guard needed.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || power is not VigorPower) return;
        if (ClearedNegativeVigor(power.Amount - amount, power.Amount))
            await PowerCmd.Apply<VigorPower>(context, Owner, Amount, Owner, null, false);
    }

    // Pure decision: a debuff cleared iff Vigor was negative and is now back to zero-or-positive.
    public static bool ClearedNegativeVigor(decimal oldAmount, decimal newAmount) =>
        oldAmount < 0m && newAmount >= 0m;
}
