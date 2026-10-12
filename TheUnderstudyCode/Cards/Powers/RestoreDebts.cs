using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Some base-game enemies take a stat from you and later give it back, so they record what they owe:
//   • The Lost / The Forgotten (PossessStrengthPower / PossessSpeedPower): steal Strength / Dexterity,
//     return the recorded amount when they die.
//   • Hunter Killer (TenderPower): -1 Strength and -1 Dexterity per card you play, restored at turn end.
// Invert turns that negative Strength/Dexterity into a buff, but the enemy still pays the full amount back
// on top, so you keep the buff AND get the stat back (smog -3, Invert -> +3, death -> +6).
//
// The fix: inverted stacks count as already returned. Whenever Invert flips N negative stacks of a stat
// on a creature, Forgive pays N off whatever restores are still owed to that creature for that stat, so
// the enemy only gives back what is still missing. This is the Strength/Dexterity version of the Thorns
// rule (PowerNeverNegativePatch): an enemy's cleanup can only undo what is still there.
//
// The Possess powers already record the amount that actually landed, so their ledger is their own
// Dictionary and Forgive edits it in place. TenderPower only counts cards played, so TenderDebtPatch keeps
// its per-stat debt here (keyed per power instance). The debt is filled and paid by lockstep hooks from
// synced state, so every client computes the same value, like PrePlannedSetup's table.
public static class RestoreDebts
{
    public sealed class StatDebt
    {
        public int Strength;
        public int Dexterity;
    }

    private static readonly ConditionalWeakTable<PowerModel, StatDebt> _tenderDebts = new();

    public static StatDebt TenderDebt(PowerModel tender) => _tenderDebts.GetOrCreateValue(tender);

    // PowerModel keeps per-instance private data in `_internalData` (GetInternalData<T> is protected).
    private static readonly AccessTools.FieldRef<PowerModel, object?> InternalData =
        AccessTools.FieldRefAccess<PowerModel, object?>("_internalData");

    // Pure: pay up to `amount` off a non-negative `debt`. Returns the new debt and how much was paid.
    public static (int newDebt, int paid) Pay(int debt, int amount)
    {
        int paid = Math.Max(0, Math.Min(Math.Max(0, debt), amount));
        return (debt - paid, paid);
    }

    // Pure: how much of a stat a single application actually took, given the value before and after it.
    // Never negative: if something turned the loss into a gain (My Own Lesson), nothing is owed back.
    public static int LandedLoss(int before, int after) => Math.Max(0, before - after);

    // `stat` is the power whose negative stacks Invert just flipped on `creature`; `converted` is how many.
    // Holders are paid in combat order (allies first, so Tender's same-turn debt before a Possess debt).
    public static void Forgive(Creature creature, PowerModel stat, int converted)
    {
        if (converted <= 0) return;
        bool isStrength = stat is StrengthPower;
        bool isDexterity = stat is DexterityPower;
        if (!isStrength && !isDexterity) return;
        if (creature.CombatState is not { } combatState) return;

        foreach (var holder in combatState.Creatures)
            foreach (var power in holder.Powers.ToList())
            {
                converted -= power switch
                {
                    TenderPower when power.Owner == creature => PayTender(TenderDebt(power), isStrength, converted),
                    PossessStrengthPower when isStrength => PayPossess(power, "stolenStrength", creature, converted),
                    PossessSpeedPower when isDexterity => PayPossess(power, "stolenDexterity", creature, converted),
                    _ => 0,
                };
                if (converted <= 0) return;
            }
    }

    private static int PayTender(StatDebt debt, bool isStrength, int amount)
    {
        int paid;
        if (isStrength) (debt.Strength, paid) = Pay(debt.Strength, amount);
        else (debt.Dexterity, paid) = Pay(debt.Dexterity, amount);
        return paid;
    }

    // The Possess ledgers store the stolen amount as a NEGATIVE decimal per player (the change they saw).
    private static int PayPossess(PowerModel power, string field, Creature creature, int amount)
    {
        var data = InternalData(power);
        if (data == null) return 0;
        if (AccessTools.Field(data.GetType(), field)?.GetValue(data) is not Dictionary<Creature, decimal> stolen)
            return 0;
        if (!stolen.TryGetValue(creature, out var owed)) return 0;

        var (newDebt, paid) = Pay((int)-owed, amount);
        stolen[creature] = -newDebt;
        return paid;
    }
}
