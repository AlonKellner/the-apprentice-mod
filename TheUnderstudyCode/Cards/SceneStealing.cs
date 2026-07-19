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
//   • GIVE: transfer up to SwapCap of the player's *most recently modified* swappable debuff to ALL enemies.
//   • TAKE: from EACH enemy, steal up to SwapCap of *that enemy's own* most recently modified swappable buff
//           (removed from the enemy, gained by the player).
// Each repeat recalculates "most recently modified" from live state — no state is carried between
// applications (giving your latest debuff empties it, so the next repeat naturally moves to the next one).
//
// "Most recently modified" recency is recorded per creature by SwapRecencyPatch (see SwapRecency); when a
// creature holds a swappable power the observer never saw change (an innate buff), selection falls back to
// registry order. "Swappable" = membership in the two curated registries below — easy to tune.
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

    // The union of swappable power entries, for SwapRecencyPatch to cheaply filter which amount changes
    // are worth recording. Lazy so ModelDb is ready.
    private static HashSet<string>? _swappableEntries;
    public static bool IsSwappableEntry(string entry) => (_swappableEntries ??= BuildSwappableEntries()).Contains(entry);

    private static HashSet<string> BuildSwappableEntries()
    {
        var set = new HashSet<string>();
        foreach (var p in SwappableDebuffs) set.Add(p.Id.Entry);
        foreach (var p in SwappableBuffs) set.Add(p.Id.Entry);
        return set;
    }

    // Pure: how much of a holding moves per application — capped at SwapCap and at what you actually have
    // (never negative). Also used for the negative portion of a sign-flip buff by passing its magnitude.
    public static int ComputeTransfer(int have) => Math.Max(0, Math.Min(have, SwapCap));

    // Pure: among candidates given in registry order, the index of the most recently modified one
    // (highest recency stamp). When none have a stamp (all long.MinValue — e.g. only pre-existing innate
    // powers), this returns 0, i.e. the first in registry order. -1 for an empty list.
    public static int SelectByRecency(IReadOnlyList<long> recencyStamps)
    {
        if (recencyStamps.Count == 0) return -1;
        int best = 0;
        for (int i = 1; i < recencyStamps.Count; i++)
            if (recencyStamps[i] > recencyStamps[best]) best = i;
        return best;
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
            await GiveLastDebuff(ctx, self, enemies);
            foreach (var enemy in enemies)
                await TakeLastBuff(ctx, self, enemy);
        }
    }

    // GIVE half: transfer up to SwapCap of the player's most recently modified swappable debuff to every
    // enemy. A sign-flip Vigor counts as a debuff while negative — nudged toward 0 on you and piled as
    // more-negative onto each enemy (the same shape the old "give negative" half used).
    private static async Task GiveLastDebuff(PlayerChoiceContext ctx, Creature self, IReadOnlyList<Creature> enemies)
    {
        var chosen = SelectDebuff(self);
        if (chosen == null) return;
        var (power, magnitude, signFlip) = chosen.Value;

        int move = ComputeTransfer(magnitude);
        if (move <= 0) return;

        if (signFlip)
        {
            await PowerCmd.Apply(ctx, Fresh(power), self, move, self, null);         // toward 0 on you
            foreach (var enemy in enemies)
                await PowerCmd.Apply(ctx, Fresh(power), enemy, -move, self, null);   // pile the negative onto each enemy
        }
        else
        {
            await PowerCmd.Apply(ctx, Fresh(power), self, -move, self, null);        // remove from you
            foreach (var enemy in enemies)
                await PowerCmd.Apply(ctx, Fresh(power), enemy, move, self, null);    // add to each enemy
        }
    }

    // TAKE half: from a single enemy, steal up to SwapCap of that enemy's own most recently modified
    // swappable buff (positive portion only), removed from the enemy and gained by the player.
    private static async Task TakeLastBuff(PlayerChoiceContext ctx, Creature self, Creature enemy)
    {
        var buff = SelectBuff(enemy);
        if (buff == null) return;

        int amt = enemy.GetPower(buff.Id)?.Amount ?? 0;
        int take = ComputeTransfer(amt);
        if (take <= 0) return;

        await PowerCmd.Apply(ctx, Fresh(buff), enemy, -take, self, null);
        await PowerCmd.Apply(ctx, Fresh(buff), self, take, self, null);
    }

    // The player's most recently modified swappable debuff currently in stock: a normal swappable debuff
    // with a positive amount, or a sign-flip buff (Vigor) with a negative amount (its magnitude given).
    // Candidates are built in registry order (debuffs, then sign-flip negatives) so SelectByRecency's
    // no-stamp fallback lands on the first registry entry you hold. Null when there is nothing to give.
    private static (PowerModel power, int magnitude, bool signFlip)? SelectDebuff(Creature self)
    {
        var candidates = new List<(PowerModel power, int magnitude, bool signFlip)>();
        foreach (var debuff in SwappableDebuffs)
        {
            int amt = self.GetPower(debuff.Id)?.Amount ?? 0;
            if (amt > 0) candidates.Add((debuff, amt, false));
        }
        foreach (var power in SignFlipBuffs)
        {
            int amt = self.GetPower(power.Id)?.Amount ?? 0;
            if (amt < 0) candidates.Add((power, -amt, true));
        }
        if (candidates.Count == 0) return null;

        var stamps = candidates.Select(c => SwapRecency.LastModified(self, c.power.Id.Entry)).ToList();
        return candidates[SelectByRecency(stamps)];
    }

    // An enemy's most recently modified swappable buff currently in stock (positive portion only, which
    // also handles AllowNegative Vigor). Candidates in registry order; null when the enemy has no swappable
    // buff to steal.
    private static PowerModel? SelectBuff(Creature enemy)
    {
        var candidates = new List<PowerModel>();
        foreach (var buff in SwappableBuffs)
        {
            int amt = enemy.GetPower(buff.Id)?.Amount ?? 0;
            if (amt > 0) candidates.Add(buff);
        }
        if (candidates.Count == 0) return null;

        var stamps = candidates.Select(b => SwapRecency.LastModified(enemy, b.Id.Entry)).ToList();
        return candidates[SelectByRecency(stamps)];
    }
}
