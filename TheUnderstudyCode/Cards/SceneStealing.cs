using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The "Swap" mechanic — sibling of EmotionalExpression (Invert). Where Invert flips your own debuffs
// into buffs on yourself, Swap trades fortunes with the enemy team. It has NO numeric magnitude: each
// application moves a single power per side, capped at SwapCap, and the card number is a REPEAT count
// ("Swap" / "Swap twice" / "Swap 3 times"):
//   • GIVE: transfer up to SwapCap of the player's *rightmost* swappable debuff to ALL enemies.
//   • TAKE: from EACH enemy, steal up to SwapCap of *that enemy's own* rightmost swappable buff
//           (removed from the enemy, gained by the player).
// Each repeat recalculates the target from live state — no state is carried between applications (giving
// your rightmost debuff empties it, so the next repeat naturally moves to the next one).
//
// "Rightmost" = the last matching entry in the creature's Powers list. That list order is part of the
// game-state checksum (CombatState hashes creature.Powers in order, un-sorted), so it is identical on
// every multiplayer client — making selection a pure function of synced state. This REPLACED a
// process-global "most recently modified" recency counter (SwapRecency), which diverged across clients:
// the counter's increment order depended on how each client interleaved different players' power changes
// during the parallel play phase, so clients could pick different targets and desync. See project memory
// "RNG determinism" / the multiplayer-determinism plan. "Swappable" = membership in the two curated
// registries below.
public static class SceneStealing
{
    // Per-application cap: at most this much of a single power moves per Swap. High enough to read as
    // "move (nearly) all of it", matching max hand size.
    public const int SwapCap = 10;

    private static IReadOnlyList<PowerModel>? _swappableDebuffs;
    private static IReadOnlyList<PowerModel>? _swappableBuffs;

    // Debuffs you transfer onto enemies. Lazy so ModelDb is ready before we resolve canonicals.
    public static IReadOnlyList<PowerModel> SwappableDebuffs => _swappableDebuffs ??= new PowerModel[]
    {
        ModelDb.Power<WeakPower>(),
        ModelDb.Power<VulnerablePower>(),
        ModelDb.Power<FrailPower>(),
        ModelDb.Power<PoisonPower>(),
        ModelDb.Power<DoomPower>(),
        ModelDb.Power<ConstrictPower>(),
        ModelDb.Power<TaintedPower>(),
        ModelDb.Power<TensionPower>(),
    };

    // Buffs you steal from enemies (their positive portion). Vigor is AllowNegative: only its positive
    // (true buff) portion is stolen here — its negative (debuff) portion is instead GIVEN to enemies, see
    // SignFlipBuffs and the sign-flip branch in GiveLastDebuff. Strength/Dexterity are deliberately NOT
    // swappable (too many enemies scale off them); they remain Invertible via EmotionalExpression.
    public static IReadOnlyList<PowerModel> SwappableBuffs => _swappableBuffs ??= new PowerModel[]
    {
        ModelDb.Power<VigorPower>(),
        ModelDb.Power<RegenPower>(),
        ModelDb.Power<ThornsPower>(),
        ModelDb.Power<ArtifactPower>(),
        ModelDb.Power<UnweakPower>(),
        ModelDb.Power<UnvulnerablePower>(),
        ModelDb.Power<UnfrailPower>(),
        ModelDb.Power<UntaintedPower>(),
        ModelDb.Power<UntensionPower>(),
        ModelDb.Power<UndoomPower>(),
    };

    // The AllowNegative buffs whose POSITIVE portion is a buff (stolen from enemies, above) and whose
    // NEGATIVE portion is a debuff (given to enemies, like Weak). Subset of SwappableBuffs; only these have
    // a meaningful negative side (Regen/Thorns/Artifact/Un- are always >= 0). Only Vigor after Strength/
    // Dexterity were dropped from the swappable set.
    private static IReadOnlyList<PowerModel>? _signFlipBuffs;
    public static IReadOnlyList<PowerModel> SignFlipBuffs => _signFlipBuffs ??= new PowerModel[]
    {
        ModelDb.Power<VigorPower>(),
    };

    // ── The shared "what Swap can give, and how" vocabulary ────────────────────────────────────────────
    // Regular Swap and Best of Both both move your debuffs onto enemies. They used to answer "which of my
    // debuffs can be given, and which direction does the number move?" in two independent places, and they
    // drifted: Swap knew a negative sign-flip Vigor is a giveable debuff, Best of Both's per-pair capture
    // did not, so Best of Both silently inverted negative Vigor without ever handing it to the enemies.
    // Everything below is THE single answer, used by both paths — see GiveableDebuffs / IsGiveable /
    // RemoveDebuffFromSelf / GiveDebuffToEnemy.

    // One giveable debuff you are holding. Magnitude is always POSITIVE (how much debuff there is),
    // regardless of how the power stores it. SignFlip marks the one-power/signed kind (Vigor), where the
    // debuff is the negative portion and "removing" it means moving the value toward zero.
    public readonly record struct DebuffHolding(PowerModel Power, int Magnitude, bool SignFlip);

    // Every debuff holding `creature` currently has that Swap is allowed to give away: normal swappable
    // debuffs in stock, plus sign-flip buffs sitting negative. THE definition of "a debuff Swap can give".
    public static List<DebuffHolding> GiveableDebuffs(Creature creature)
    {
        var holdings = new List<DebuffHolding>();
        foreach (var debuff in SwappableDebuffs)
        {
            int amt = creature.GetPower(debuff.Id)?.Amount ?? 0;
            if (amt > 0) holdings.Add(new DebuffHolding(debuff, amt, false));
        }
        foreach (var power in SignFlipBuffs)
        {
            int amt = creature.GetPower(power.Id)?.Amount ?? 0;
            if (amt < 0) holdings.Add(new DebuffHolding(power, -amt, true));
        }
        return holdings;
    }

    // Whether a holding may be given to enemies at all — the same membership rule GiveableDebuffs applies,
    // asked about one power. Best of Both uses this to decide give-and-invert vs invert-only (Strength and
    // Dexterity are invertible but deliberately not swappable).
    public static bool IsGiveable(DebuffHolding holding) =>
        (holding.SignFlip ? SignFlipBuffs : SwappableDebuffs).Any(p => p.Id.Equals(holding.Power.Id));

    // The direction a give moves the number, and the ONLY place that knows it. A normal debuff is removed
    // from you and added to the enemy; a sign-flip power is nudged up toward zero on you and piled further
    // negative on the enemy. Split out as plain ints so the sign table itself is unit-testable.
    public static int SelfGiveDelta(bool signFlip, int amount) => signFlip ? amount : -amount;
    public static int EnemyGiveDelta(bool signFlip, int amount) => signFlip ? -amount : amount;

    // The two staged moves of a give, built from the deltas above.
    public static PowerMove RemoveDebuffFromSelf(DebuffHolding holding, Creature self, int amount) =>
        new(holding.Power, self, SelfGiveDelta(holding.SignFlip, amount));

    public static PowerMove GiveDebuffToEnemy(DebuffHolding holding, Creature enemy, int amount) =>
        new(holding.Power, enemy, EnemyGiveDelta(holding.SignFlip, amount));

    // Pure: how much of a holding moves per application — capped at SwapCap and at what you actually have
    // (never negative). Also used for the negative portion of a sign-flip buff by passing its magnitude.
    public static int ComputeTransfer(int have) => Math.Max(0, Math.Min(have, SwapCap));

    // Pure: among candidates, the index of the one that sits RIGHTMOST (last) in the creature's Powers
    // list. positions[i] is candidate i's index within creature.Powers (-1 if absent); the highest wins,
    // ties broken toward the later candidate. Powers-list order is checksummed and synced, so this is
    // deterministic on every client. Returns -1 for an empty candidate list.
    public static int SelectRightmost(IReadOnlyList<int> positions)
    {
        if (positions.Count == 0) return -1;
        int best = 0;
        for (int i = 1; i < positions.Count; i++)
            if (positions[i] >= positions[best]) best = i;
        return best;
    }

    // Index of the power with this id within the creature's Powers list (the synced, checksummed order),
    // or -1 if the creature doesn't hold it.
    private static int PowerPosition(Creature creature, ModelId id)
    {
        var powers = creature.Powers;
        for (int i = 0; i < powers.Count; i++)
            if (powers[i].Id.Equals(id)) return i;
        return -1;
    }

    // The registries hold CANONICAL powers (ModelDb.Power<T>()) — fine for reading Id/amount, but
    // PowerCmd.Apply's new-application path calls power.AssertMutable() and BaseLib's SelfApplyDebuffPatch
    // re-invokes Apply with the same instance, so every apply must get a FRESH mutable clone (mirroring
    // what PowerCmd.Apply<T> does internally). Passing the raw canonical throws "Canonical model … used in
    // incorrect place" the moment a swappable base power is actually moved.
    private static PowerModel Fresh(PowerModel canonical) => (PowerModel)canonical.MutableClone();

    public static async Task Swap(PlayerChoiceContext ctx, Creature self, int repeats)
    {
        if (repeats <= 0) return;
        var enemies = self.CombatState!.HittableEnemies.ToList();
        for (int r = 0; r < repeats; r++)
        {
            var plan = new SwapPlan();
            CaptureGiveMostRecent(plan, self, enemies);
            CaptureTake(plan, self, enemies, SwapCap);
            await ExecutePlan(ctx, self, plan);
        }
    }

    // ── The shared capture -> remove -> apply pipeline ─────────────────────────────────────────────────
    // Both Swap and Best of Both move powers between you and the enemies, and both must clear BOTH sides
    // before applying anything, so interacting powers swap instead of cancelling (an enemy's Artifact that
    // would eat an incoming debuff; a Weak/Unweak or other buff/debuff pair). So each CAPTUREs its moves
    // from the current state into a SwapPlan, then ExecutePlan REMOVES from both sides and only then APPLIES
    // to both sides. Best of Both adds Invert moves via InvertiblePair.CaptureGiveAndInvert; the take half
    // (CaptureTake) is identical for both.

    // One planned power change: at execute time, apply Delta of Power to Target (Delta < 0 = removal).
    public readonly record struct PowerMove(PowerModel Power, Creature Target, int Delta);

    public sealed class SwapPlan
    {
        public readonly List<PowerMove> Removes = new();
        public readonly List<PowerMove> Applies = new();
    }

    public static async Task ExecutePlan(PlayerChoiceContext ctx, Creature self, SwapPlan plan)
    {
        foreach (var m in plan.Removes) await PowerCmd.Apply(ctx, Fresh(m.Power), m.Target, m.Delta, self, null);
        foreach (var m in plan.Applies) await PowerCmd.Apply(ctx, Fresh(m.Power), m.Target, m.Delta, self, null);
    }

    // GIVE (regular Swap): your most recently modified swappable debuff, transferred to every enemy. A
    // sign-flip Vigor counts as a debuff while negative — nudged toward 0 on you, piled negative on enemies.
    private static void CaptureGiveMostRecent(SwapPlan plan, Creature self, IReadOnlyList<Creature> enemies)
    {
        var chosen = SelectDebuff(self);
        if (chosen == null) return;
        int move = ComputeTransfer(chosen.Value.Magnitude);
        if (move <= 0) return;

        plan.Removes.Add(RemoveDebuffFromSelf(chosen.Value, self, move));
        foreach (var enemy in enemies) plan.Applies.Add(GiveDebuffToEnemy(chosen.Value, enemy, move));
    }

    // TAKE (shared by Swap and Best of Both): from each enemy, steal that enemy's most recently modified
    // swappable buff (up to `cap`), removed from the enemy and gained by you.
    public static void CaptureTake(SwapPlan plan, Creature self, IReadOnlyList<Creature> enemies, int cap)
    {
        foreach (var enemy in enemies)
        {
            var buff = SelectBuff(enemy);
            if (buff == null) continue;
            int take = Math.Min(enemy.GetPower(buff.Id)?.Amount ?? 0, cap);
            if (take <= 0) continue;
            plan.Removes.Add(new PowerMove(buff, enemy, -take));
            plan.Applies.Add(new PowerMove(buff, self, take));
        }
    }

    // The player's RIGHTMOST giveable debuff: among everything GiveableDebuffs reports, the one sitting
    // last in the creature's Powers list wins (SelectRightmost). Null when there is nothing to give.
    // Regular Swap moves exactly one holding per application; Best of Both moves them all — but both draw
    // from the same GiveableDebuffs set, so neither can consider a debuff the other doesn't.
    private static DebuffHolding? SelectDebuff(Creature self)
    {
        var candidates = GiveableDebuffs(self);
        if (candidates.Count == 0) return null;

        var positions = candidates.Select(c => PowerPosition(self, c.Power.Id)).ToList();
        return candidates[SelectRightmost(positions)];
    }

    // An enemy's RIGHTMOST swappable buff currently in stock (positive portion only, which also handles
    // AllowNegative Vigor): the one sitting last in the enemy's Powers list. Null when the enemy has no
    // swappable buff to steal.
    private static PowerModel? SelectBuff(Creature enemy)
    {
        var candidates = new List<PowerModel>();
        foreach (var buff in SwappableBuffs)
        {
            int amt = enemy.GetPower(buff.Id)?.Amount ?? 0;
            if (amt > 0) candidates.Add(buff);
        }
        if (candidates.Count == 0) return null;

        var positions = candidates.Select(b => PowerPosition(enemy, b.Id)).ToList();
        return candidates[SelectRightmost(positions)];
    }
}
