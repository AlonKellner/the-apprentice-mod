using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Shared plumbing for the "every N ..." counter relics (Cue Light, Greasepaint): the count-up ring
// (ShowCounter/DisplayAmount), a pure threshold fold, per-combat reset, and a helper that applies a
// debuff to a random enemy. Subclasses wire their own trigger to Bump() and, when it fires, call
// ApplyToRandomEnemy from an async hook that has a real PlayerChoiceContext.
[Pool(typeof(TheUnderstudyRelicPool))]
public abstract class UnderstudyCounterRelic : CustomRelicModel
{
    protected abstract int Threshold { get; }

    // Progress toward the next fire (0..Threshold-1). Shown on the relic's count-up ring.
    protected int Counter { get; private set; }

    public override bool ShowCounter => true;
    public override int DisplayAmount => Counter;

    // Pure: fold `add` into `counter` at `threshold`; returns how many times it crosses the threshold
    // and the leftover progress. Directly unit-testable (no engine dependency).
    public static (int fires, int remainder) Advance(int counter, int add, int threshold)
    {
        int total = counter + add;
        return (total / threshold, total % threshold);
    }

    // Records one tick; returns how many times the threshold fired (0 or 1 for a single tick).
    protected int Bump()
    {
        var (fires, remainder) = Advance(Counter, 1, Threshold);
        Counter = remainder;
        InvokeDisplayAmountChanged();
        return fires;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Counter = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    // Apply `amount` of a debuff power to a random hittable enemy (deterministic via the run's RNG).
    protected async Task ApplyToRandomEnemy<TPower>(PlayerChoiceContext context, decimal amount)
        where TPower : PowerModel
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;
        var enemies = combat.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target == null) return;
        Flash();
        await PowerCmd.Apply<TPower>(context, target, amount, Owner.Creature, null, false);
    }
}
