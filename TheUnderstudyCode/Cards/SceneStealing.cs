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

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The "Swap" mechanic — sibling of EmotionalExpression (Invert). Where Invert flips your own
// debuffs into buffs on yourself, Swap trades fortunes with the enemy team:
//   Swap X: give X of each swappable debuff you have to ALL enemies (transfer — remove from you,
//           apply to each enemy); take X of each swappable buff from ALL enemies (remove up to X
//           from each enemy, and you gain the TOTAL removed).
// "Swappable" = universal, owner-relative buffs/debuffs that work on any creature (see the Swap
// verdict table in the plan). The set is just the two curated registries below — easy to tune.
public static class SceneStealing
{
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

    // Buffs you steal from enemies (their positive portion). Strength/Dexterity/Vigor are AllowNegative:
    // only their positive (true buff) portion is stolen here — their negative (debuff) portion is instead
    // GIVEN to enemies, see SignFlipBuffs / the give-negative loop in SwapEach.
    public static IReadOnlyList<PowerModel> SwappableBuffs => _swappableBuffs ??= new PowerModel[]
    {
        ModelDb.Power<StrengthPower>(),
        ModelDb.Power<DexterityPower>(),
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

    // The AllowNegative buffs (Strength/Dexterity/Vigor) — their POSITIVE portion is a buff (stolen from
    // enemies, above) and their NEGATIVE portion is a debuff (given to enemies, like Weak). Subset of
    // SwappableBuffs; only these have a meaningful negative side (Regen/Thorns/Artifact/Un- are always >= 0).
    private static IReadOnlyList<PowerModel>? _signFlipBuffs;
    public static IReadOnlyList<PowerModel> SignFlipBuffs => _signFlipBuffs ??= new PowerModel[]
    {
        ModelDb.Power<StrengthPower>(),
        ModelDb.Power<DexterityPower>(),
        ModelDb.Power<VigorPower>(),
    };

    // Pure: how much of a debuff you hold (`have`) transfers per Swap X — capped at X and at what you
    // actually have (never negative). Also used for the negative portion of a sign-flip buff by passing
    // its magnitude (-have): e.g. -6 Vigor, Swap 6 => ComputeTransfer(6, 6) = 6.
    public static int ComputeTransfer(int have, int x) => Math.Max(0, Math.Min(have, x));

    // Pure: total stolen across a set of enemy amounts, taking up to X (positive only) from each.
    public static int ComputeSteal(IEnumerable<int> enemyAmounts, int x) =>
        enemyAmounts.Sum(a => Math.Max(0, Math.Min(a, x)));

    // The SwappableDebuffs/SwappableBuffs registries hold CANONICAL powers (ModelDb.Power<T>()) — fine
    // for reading Id/amount, but PowerCmd.Apply's new-application path calls power.AssertMutable(), and
    // BaseLib's SelfApplyDebuffPatch postfix re-invokes Apply with the same instance. So every apply
    // must get a FRESH mutable clone, mirroring what PowerCmd.Apply<T> does internally. Passing the raw
    // canonical throws "Canonical model … used in incorrect place" the moment a swappable base debuff
    // (e.g. Weak from Constant Struggle) is actually transferred.
    private static PowerModel Fresh(PowerModel canonical) => (PowerModel)canonical.MutableClone();

    public static async Task SwapEach(PlayerChoiceContext ctx, Creature self, int x)
    {
        if (x <= 0) return;
        var enemies = self.CombatState!.HittableEnemies;

        // GIVE — transfer each swappable debuff you have onto every enemy.
        foreach (var debuff in SwappableDebuffs)
        {
            int have = self.GetPower(debuff.Id)?.Amount ?? 0;
            int take = ComputeTransfer(have, x);
            if (take <= 0) continue;
            await PowerCmd.Apply(ctx, Fresh(debuff), self, -take, self, null);
            foreach (var enemy in enemies)
                await PowerCmd.Apply(ctx, Fresh(debuff), enemy, take, self, null);
        }

        // GIVE (negative side of sign-flip buffs) — a NEGATIVE Strength/Dexterity/Vigor is a debuff on
        // you, so Swap hands it to every enemy too, exactly like Weak (up to X of the magnitude). This is
        // the give-half that pairs with the positive-steal below (e.g. your -6 Vigor + enemy -6 Vigor
        // and Swap 6 => you 0, enemy -12).
        foreach (var power in SignFlipBuffs)
        {
            int have = self.GetPower(power.Id)?.Amount ?? 0;
            int give = ComputeTransfer(-have, x); // magnitude of the negative portion, capped at X (0 if have >= 0)
            if (give <= 0) continue;
            await PowerCmd.Apply(ctx, Fresh(power), self, give, self, null); // remove it from you (toward 0)
            foreach (var enemy in enemies)
                await PowerCmd.Apply(ctx, Fresh(power), enemy, -give, self, null); // pile the negative onto each enemy
        }

        // TAKE (steal, summed) — remove up to X of each swappable buff from each enemy; you gain the total.
        foreach (var buff in SwappableBuffs)
        {
            int stolen = 0;
            foreach (var enemy in enemies)
            {
                int amt = enemy.GetPower(buff.Id)?.Amount ?? 0;
                int take = Math.Max(0, Math.Min(amt, x)); // positive only — handles AllowNegative Str/Dex/Vigor
                if (take <= 0) continue;
                await PowerCmd.Apply(ctx, Fresh(buff), enemy, -take, self, null);
                stolen += take;
            }
            if (stolen > 0)
                await PowerCmd.Apply(ctx, Fresh(buff), self, stolen, self, null);
        }
    }
}
