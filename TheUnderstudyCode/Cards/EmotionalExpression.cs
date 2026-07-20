using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public static class EmotionalExpression
{
    public static int CountUniqueDebuffTypes(int weakAmt, int vulAmt) =>
        (weakAmt > 0 ? 1 : 0) + (vulAmt > 0 ? 1 : 0);

    // The one shared cancellation primitive every Un-X power's bidirectional
    // TryModifyPowerAmountReceived leans on, in both directions, for all 6 pairs: reduce an
    // incoming gain of `applied` by however much of the opposing side (`available`) currently
    // exists, consuming that much of it in the process.
    public static (int reducedAmount, int consumed) ComputeWeakCancellation(int weakApplied, int unweakAvailable)
    {
        int consumed = Math.Min(weakApplied, unweakAvailable);
        return (weakApplied - consumed, consumed);
    }

    // Apply Weak to self. InvertTrackerPower's bidirectional interception (canonicalPower is
    // WeakPower) reduces this by any existing Unweak stock and consumes it — no local netting
    // needed here.
    public static async Task ApplyWeakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, creature, stacks, creature, card, false);    }

    // Apply Vulnerable to self. Mirror of ApplyWeakToSelf.
    public static async Task ApplyVulnerableToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, creature, stacks, creature, card, false);    }

    // Apply Unweak to self. Mirror of ApplyWeakToSelf (Unweak is the buff side of the same pair).
    public static async Task ApplyUnweakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<UnweakPower>(ctx, creature, stacks, creature, card, false);    }

    // Convert up to max WeakPower to UnweakPower. The raw removeAmount (not a pre-reduced value)
    // is what's granted to Unweak — InvertTrackerPower's interception (canonicalPower is
    // UnweakPower) reduces that raw amount by whatever Weak remains after the removal above, live,
    // at the moment this call (and each Double Time repeat of it) actually lands. This is what makes
    // the amount of cancellation depend on how much debuff is left over after a capped Invert,
    // instead of a single precomputed net.
    public static async Task<int> ConvertWeakToUnweak(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        int removeAmount = Math.Min(curWeak, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<WeakPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curWeak - removeAmount, creature.GetPowerAmount<WeakPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertWeakToUnweak), "Weak after removal");
        await PowerCmd.Apply<UnweakPower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // Mirror of ConvertWeakToUnweak for Vulnerable/Unvulnerable.
    public static async Task<int> ConvertVulnerableToUnvulnerable(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        int removeAmount = Math.Min(curVul, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<VulnerablePower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curVul - removeAmount, creature.GetPowerAmount<VulnerablePower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertVulnerableToUnvulnerable), "Vulnerable after removal");
        await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // Remove up to max Weak from source and apply to target.
    public static async Task TransferWeakTo(PlayerChoiceContext ctx, Creature source, Creature target, CardModel? card, int max = int.MaxValue)
    {
        int weakAmount = Math.Min(source.GetPowerAmount<WeakPower>(), max);
        if (weakAmount <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, source, -weakAmount, source, card, false);
        await PowerCmd.Apply<WeakPower>(ctx, target, weakAmount, source, card, false);
    }

    // Remove up to max Vulnerable from source and apply to target.
    public static async Task TransferVulnerableTo(PlayerChoiceContext ctx, Creature source, Creature target, CardModel? card, int max = int.MaxValue)
    {
        int vulAmount = Math.Min(source.GetPowerAmount<VulnerablePower>(), max);
        if (vulAmount <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, source, -vulAmount, source, card, false);
        await PowerCmd.Apply<VulnerablePower>(ctx, target, vulAmount, source, card, false);
    }

    // Transfer debuffs (Weak, Vulnerable, negative Strength) from source to a single target.
    // maxEach caps each of Weak and Vulnerable independently; negative Strength is always fully transferred.
    public static async Task TransferDebuffsTo(PlayerChoiceContext ctx, Creature source, Creature target, CardModel? card, int maxEach = int.MaxValue)
    {
        await TransferWeakTo(ctx, source, target, card, maxEach);
        await TransferVulnerableTo(ctx, source, target, card, maxEach);

        int strengthAmount = source.GetPowerAmount<StrengthPower>();
        if (strengthAmount < 0)
        {
            await PowerCmd.Apply<StrengthPower>(ctx, source, -strengthAmount, source, card, false);
            await PowerCmd.Apply<StrengthPower>(ctx, target, strengthAmount, source, card, false);
        }
    }

    // Shaken/Unshaken — same shape as Weak/Unweak.

    public static async Task ApplyShakenToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<ShakenPower>(ctx, creature, stacks, creature, card, false);    }

    public static async Task<int> ConvertShakenToUnshaken(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curShaken = creature.GetPowerAmount<ShakenPower>();
        int removeAmount = Math.Min(curShaken, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<ShakenPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curShaken - removeAmount, creature.GetPowerAmount<ShakenPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertShakenToUnshaken), "Shaken after removal");
        await PowerCmd.Apply<UnshakenPower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // Limited/Unlimited — same shape as Weak/Unweak, throttling draw instead of dealt damage.

    public static async Task ApplyLimitedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, stacks, creature, card, false);    }

    public static async Task<int> ConvertLimitedToUnlimited(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curLimited = creature.GetPowerAmount<LimitedPower>();
        int removeAmount = Math.Min(curLimited, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curLimited - removeAmount, creature.GetPowerAmount<LimitedPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertLimitedToUnlimited), "Limited after removal");
        await PowerCmd.Apply<UnlimitedPower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // Jaded/Unjaded — same shape as Limited/Unlimited, throttling next turn's Energy instead of draw.

    public static async Task ApplyJadedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<JadedPower>(ctx, creature, stacks, creature, card, false);    }

    public static async Task<int> ConvertJadedToUnjaded(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curJaded = creature.GetPowerAmount<JadedPower>();
        int removeAmount = Math.Min(curJaded, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<JadedPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curJaded - removeAmount, creature.GetPowerAmount<JadedPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertJadedToUnjaded), "Jaded after removal");
        await PowerCmd.Apply<UnjadedPower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // Frail/Unfrail — same shape as Weak/Unweak, reducing Block gain instead of dealt damage.
    // No Understudy card applies Frail directly; this exists purely so Invert (and the
    // Invert-each variant on StrikeAPose) behaves correctly if something external inflicts it.

    public static async Task ApplyFrailToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<FrailPower>(ctx, creature, stacks, creature, card, false);    }

    public static async Task<int> ConvertFrailToUnfrail(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curFrail = creature.GetPowerAmount<FrailPower>();
        int removeAmount = Math.Min(curFrail, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<FrailPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curFrail - removeAmount, creature.GetPowerAmount<FrailPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertFrailToUnfrail), "Frail after removal");
        await PowerCmd.Apply<UnfrailPower>(ctx, creature, removeAmount, creature, null, false);        return removeAmount;
    }

    // ── Universal invertible pairs (also Swappable) ────────────────────────────────────────────
    //
    // Tainted/Untainted and Tension/Untension are same-shape debuff/buff pairs like Weak/Unweak, and
    // Vigor is a sign-flip power like Strength/Dexterity. Unlike the eight InvertibleDebuff-enum
    // pairs above, these are universal (they live on any creature) and are handled outside the enum:
    // InvertEach/InvertEachWithBonus fan out to them explicitly (see below), so Second Lesson's
    // enum-driven pair pool and InvertTrackerPower's cancellation are deliberately left untouched.

    // Apply Tension to self (mirror of ApplyShakenToSelf). Tension is the debuff side of the
    // Tension/Untension universal pair — the self-debuff downside that Tension cards carry.
    public static async Task ApplyTensionToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<TensionPower>(ctx, creature, stacks, creature, card, false);
    }

    public static async Task<int> ConvertTaintedToUntainted(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int cur = creature.GetPowerAmount<TaintedPower>();
        int removeAmount = Math.Min(cur, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<TaintedPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UntaintedPower>(ctx, creature, removeAmount, creature, null, false);
        return removeAmount;
    }

    public static async Task<int> ConvertTensionToUntension(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int cur = creature.GetPowerAmount<TensionPower>();
        int removeAmount = Math.Min(cur, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<TensionPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UntensionPower>(ctx, creature, removeAmount, creature, null, false);
        return removeAmount;
    }

    // Vigor sign-flip — identical to Strength/Dexterity (negative Vigor -> positive), enabled by
    // VigorAllowNegativePatch. Un-X powers are all buffs by convention, so there is no "Unvigor".
    public static async Task<int> InvertVigorSign(PlayerChoiceContext ctx, Creature creature, int max)
    {
        int cur = creature.GetPowerAmount<VigorPower>();
        var (converted, _) = ComputeSignFlip(cur, max);
        if (converted <= 0) return 0;
        await PowerCmd.Apply<VigorPower>(ctx, creature, 2 * converted, creature, null, false);
        return converted;
    }

    // Whether Invert would have anything to act on right now — any invertible pair with a debuff present.
    // Derived from the registry. Used by relevance highlighting on StrikeAPose/RunThrough/EnjoyTheRide/
    // RollWithIt/OwnIt/LivingTheDream.
    public static bool HasAnyInvertibleDebuffPresent(Creature creature) =>
        InvertiblePairs.All.Any(p => p.HasDebuffPresent(creature));

    // Invert up to `maxEach` stacks of every invertible pair the creature currently has a debuff of
    // (StrikeAPose's "each invertible debuff you have"). Iterates the single registry — see InvertiblePairs.
    public static async Task InvertEach(PlayerChoiceContext ctx, Creature creature, int maxEach)
    {
        foreach (var pair in InvertiblePairs.All)
            await pair.Invert(ctx, creature, maxEach);
    }

    // Like InvertEach, but for each pair actually inverted, re-gain that same buff `repeats` more times as
    // separate applications (Everything I've Got / Own It). Separate applications (not one lump sum) so
    // once-per-application triggers like Full Voice see `repeats` distinct events per pair, matching the
    // Double Time repeat idiom.
    public static async Task InvertEachWithBonus(PlayerChoiceContext ctx, Creature creature, int invertMax, int repeats)
    {
        foreach (var pair in InvertiblePairs.All)
        {
            int converted = await pair.Invert(ctx, creature, invertMax);
            if (converted <= 0) continue;
            for (int i = 0; i < repeats; i++)
                await pair.ApplyBuffSide(ctx, creature, converted);
        }
    }

    // ── Strength/Dexterity same-Power sign-flip ─────────────────────────────────────────────
    //
    // Unlike the 6 pairs above, Strength/Dexterity have no separate "Un-" Power — only negative
    // stacks on the same Power convert, positive stacks are untouched. Confirmed formula: for
    // Invert N against a Power currently at value V, convert = min(N, max(0, -V)),
    // newValue = V + 2*convert (each converted stack removes 1 negative and adds 1 positive).
    // Structurally independent of the Un-X redesign above (single power, no leftover-cancellation-
    // with-a-second-power concept) — confirmed to need no changes.
    public static (int converted, int newValue) ComputeSignFlip(int current, int max)
    {
        int converted = Math.Min(max, Math.Max(0, -current));
        return (converted, current + 2 * converted);
    }

    public static async Task<int> InvertStrengthSign(PlayerChoiceContext ctx, Creature creature, int max)
    {
        int cur = creature.GetPowerAmount<StrengthPower>();
        var (converted, _) = ComputeSignFlip(cur, max);
        if (converted <= 0) return 0;
        await PowerCmd.Apply<StrengthPower>(ctx, creature, 2 * converted, creature, null, false);
        return converted;
    }

    public static async Task<int> InvertDexteritySign(PlayerChoiceContext ctx, Creature creature, int max)
    {
        int cur = creature.GetPowerAmount<DexterityPower>();
        var (converted, _) = ComputeSignFlip(cur, max);
        if (converted <= 0) return 0;
        await PowerCmd.Apply<DexterityPower>(ctx, creature, 2 * converted, creature, null, false);
        return converted;
    }

    // ── Second Lesson's Reward/Punish priority-based pair selection ────────────────────────────
    //
    // Every turn, Rewarded/Punished each apply N stacks (N = their own Amount) to one of the 8
    // invertible pairs, chosen by priority rather than pure chance: Reward prefers an untouched
    // pair, then one that already has the buff (top it up), and only as a last resort one that
    // currently has the debuff; Punish mirrors this (untouched, then already-debuffed, then
    // override an existing buff). Within whichever priority tier is non-empty, the actual pair is
    // picked at random — this keeps "applies N of *a random* buff/debuff" true while still
    // respecting the priority rule.

    public enum PairCategory
    {
        None,
        BuffPresent,
        DebuffPresent
    }

    public static readonly IReadOnlyList<PairCategory> RewardPriority =
        new[] { PairCategory.None, PairCategory.BuffPresent, PairCategory.DebuffPresent };

    public static readonly IReadOnlyList<PairCategory> PunishPriority =
        new[] { PairCategory.None, PairCategory.DebuffPresent, PairCategory.BuffPresent };

    // Pure: given a pair's current debuff-side/buff-side stack amounts, which category it's in.
    // In steady state a pair is never both (InvertTrackerPower's bidirectional cancellation nets
    // them down to at most one side), but debuff wins if both are somehow nonzero rather than
    // silently picking one.
    public static PairCategory Categorize(int debuffAmount, int buffAmount) =>
        debuffAmount > 0 ? PairCategory.DebuffPresent : (buffAmount > 0 ? PairCategory.BuffPresent : PairCategory.None);

    // Pure: same 3-way categorization for Strength/Dexterity, whose sign already encodes buff vs
    // debuff on a single Power (see the sign-flip section above).
    public static PairCategory CategorizeSigned(int amount) =>
        amount < 0 ? PairCategory.DebuffPresent : (amount > 0 ? PairCategory.BuffPresent : PairCategory.None);

    // The full pair->category map for a creature (every InvertiblePair), for Reward's selection (or
    // Punish's, before the First-Lesson exclusion below). Derived from the single registry.
    public static Dictionary<InvertiblePair, PairCategory> BuildCategories(Creature creature)
    {
        var result = new Dictionary<InvertiblePair, PairCategory>();
        foreach (var pair in InvertiblePairs.All)
            result[pair] = pair.Categorize(creature);
        return result;
    }

    // While The First Lesson is active, TheFirstLessonPower zeroes any incoming Weak/Vulnerable gain
    // outright, so Punish applying either would be a silent no-op — exclude those pairs from Punish's
    // candidate pool entirely (not just deprioritize). Reward's pool is never filtered this way: The First
    // Lesson only blocks the debuff side, and Unweak/Unvulnerable as buffs are never blocked.
    public static IReadOnlyDictionary<InvertiblePair, PairCategory> ExcludeForPunishIfFirstLessonActive(
        IReadOnlyDictionary<InvertiblePair, PairCategory> categories, bool firstLessonActive)
    {
        if (!firstLessonActive) return categories;
        return categories.Where(kv => !kv.Key.IsWeakOrVulnerable).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    // Searches priorityOrder for the first non-empty category, then hands its candidates to picker (the
    // randomness lives outside this pure function, injected as a testing seam). Generic over the pair key.
    public static InvertiblePair PickByPriority(
        IReadOnlyDictionary<InvertiblePair, PairCategory> categories,
        IReadOnlyList<PairCategory> priorityOrder,
        Func<IReadOnlyList<InvertiblePair>, InvertiblePair> picker)
    {
        foreach (var category in priorityOrder)
        {
            var candidates = categories.Where(kv => kv.Value == category).Select(kv => kv.Key).ToList();
            if (candidates.Count > 0) return picker(candidates);
        }
        throw new InvalidOperationException("PickByPriority: categories did not contain any of the given priority tiers.");
    }
}
