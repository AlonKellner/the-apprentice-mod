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

// Every debuff/buff pair that the Invert keyword can act on. Weak/Vulnerable/Shaken/Limited/Jaded
// are this deck's own flavors; Frail/Strength/Dexterity are never applied by an Understudy card,
// but Invert must still recognize them if something external (an enemy, a relic) inflicts them.
public enum InvertibleDebuff
{
    Weak,
    Vulnerable,
    Shaken,
    Limited,
    Jaded,
    Frail,
    Strength,
    Dexterity
}

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

    // Which of the 6 same-shape X/Un-X pairs canonicalPower belongs to, and whether the gain lands on
    // the debuff (X) side or buff (Un-X) side. Null for anything else, including Strength/Dexterity —
    // those are sign-flip powers with no separate Un-X type, handled by a distinct mechanism (see
    // My Own Lesson's use of this in InvertTrackerPower, which branches on StrengthPower/DexterityPower
    // separately before falling back to this lookup).
    public static (InvertibleDebuff Debuff, bool IsDebuffSide)? IdentifyPair(PowerModel canonicalPower) => canonicalPower switch
    {
        WeakPower => (InvertibleDebuff.Weak, true),
        UnweakPower => (InvertibleDebuff.Weak, false),
        VulnerablePower => (InvertibleDebuff.Vulnerable, true),
        UnvulnerablePower => (InvertibleDebuff.Vulnerable, false),
        ShakenPower => (InvertibleDebuff.Shaken, true),
        UnshakenPower => (InvertibleDebuff.Shaken, false),
        LimitedPower => (InvertibleDebuff.Limited, true),
        UnlimitedPower => (InvertibleDebuff.Limited, false),
        JadedPower => (InvertibleDebuff.Jaded, true),
        UnjadedPower => (InvertibleDebuff.Jaded, false),
        FrailPower => (InvertibleDebuff.Frail, true),
        UnfrailPower => (InvertibleDebuff.Frail, false),
        _ => null
    };

    // Apply Weak to self. InvertTrackerPower's bidirectional interception (canonicalPower is
    // WeakPower) reduces this by any existing Unweak stock and consumes it — no local netting
    // needed here.
    public static async Task ApplyWeakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Weak);
    }

    // Apply Vulnerable to self. Mirror of ApplyWeakToSelf.
    public static async Task ApplyVulnerableToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Vulnerable);
    }

    // Apply Unweak to self. Mirror of ApplyWeakToSelf (Unweak is the buff side of the same pair).
    public static async Task ApplyUnweakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<UnweakPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Weak);
    }

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
        await PowerCmd.Apply<UnweakPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Weak);
        return removeAmount;
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
        await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Vulnerable);
        return removeAmount;
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
        await PowerCmd.Apply<ShakenPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Shaken);
    }

    public static async Task<int> ConvertShakenToUnshaken(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curShaken = creature.GetPowerAmount<ShakenPower>();
        int removeAmount = Math.Min(curShaken, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<ShakenPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curShaken - removeAmount, creature.GetPowerAmount<ShakenPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertShakenToUnshaken), "Shaken after removal");
        await PowerCmd.Apply<UnshakenPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Shaken);
        return removeAmount;
    }

    // Limited/Unlimited — same shape as Weak/Unweak, throttling draw instead of dealt damage.

    public static async Task ApplyLimitedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Limited);
    }

    public static async Task<int> ConvertLimitedToUnlimited(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curLimited = creature.GetPowerAmount<LimitedPower>();
        int removeAmount = Math.Min(curLimited, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curLimited - removeAmount, creature.GetPowerAmount<LimitedPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertLimitedToUnlimited), "Limited after removal");
        await PowerCmd.Apply<UnlimitedPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Limited);
        return removeAmount;
    }

    // Jaded/Unjaded — same shape as Limited/Unlimited, throttling next turn's Energy instead of draw.

    public static async Task ApplyJadedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<JadedPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Jaded);
    }

    public static async Task<int> ConvertJadedToUnjaded(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curJaded = creature.GetPowerAmount<JadedPower>();
        int removeAmount = Math.Min(curJaded, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<JadedPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curJaded - removeAmount, creature.GetPowerAmount<JadedPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertJadedToUnjaded), "Jaded after removal");
        await PowerCmd.Apply<UnjadedPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Jaded);
        return removeAmount;
    }

    // Frail/Unfrail — same shape as Weak/Unweak, reducing Block gain instead of dealt damage.
    // No Understudy card applies Frail directly; this exists purely so Invert (and the
    // Invert-each variant on Coda) behaves correctly if something external inflicts it.

    public static async Task ApplyFrailToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<FrailPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Frail);
    }

    public static async Task<int> ConvertFrailToUnfrail(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curFrail = creature.GetPowerAmount<FrailPower>();
        int removeAmount = Math.Min(curFrail, max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<FrailPower>(ctx, creature, -removeAmount, creature, null, false);
        Invariants.CheckEqual(curFrail - removeAmount, creature.GetPowerAmount<FrailPower>(),
            nameof(EmotionalExpression) + "." + nameof(ConvertFrailToUnfrail), "Frail after removal");
        await PowerCmd.Apply<UnfrailPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Frail);
        return removeAmount;
    }

    // Whether Invert would have anything at all to act on right now — all 8 invertible pairs
    // (the 5 self-debuffs plus Frail/Strength/Dexterity, matching PickDebuffToInvert's/IsPresent's
    // scope below). Used by relevance highlighting on Coda/Reprise/SteadyNow/TakeABreath/EverythingIveGot.
    public static bool HasAnyInvertibleDebuffPresent(
        int weak, int vulnerable, int shaken, int limited, int jaded, int frail, int strength, int dexterity) =>
        weak > 0 || vulnerable > 0 || shaken > 0 || limited > 0 || jaded > 0 || frail > 0
        || strength < 0 || dexterity < 0;

    public static bool HasAnyInvertibleDebuffPresent(Creature creature) => HasAnyInvertibleDebuffPresent(
        creature.GetPowerAmount<WeakPower>(),
        creature.GetPowerAmount<VulnerablePower>(),
        creature.GetPowerAmount<ShakenPower>(),
        creature.GetPowerAmount<LimitedPower>(),
        creature.GetPowerAmount<JadedPower>(),
        creature.GetPowerAmount<FrailPower>(),
        creature.GetPowerAmount<StrengthPower>(),
        creature.GetPowerAmount<DexterityPower>());

    // ── Invert dispatcher ────────────────────────────────────────────────────────────────────
    //
    // "Last modified invertible debuff" tracking, combat-scoped like TenseModifier's own static
    // counter. `_modificationOrder` holds every category touched this combat, most-recent-first.
    // Updated two ways: (1) every Apply/Convert method above calls RecordModified directly for
    // this deck's own self-application/inversion, and (2) InvertTrackerPower — a hidden Power
    // silently auto-attached to the player (see UnderstudyCard.AfterPlayerTurnStartLate, mirroring
    // PlannedCounterPower) — observes MegaCrit.Sts2.Core.Hooks.Hook.AfterPowerAmountChanged, a
    // global broadcast fired for every power amount change regardless of source, and calls
    // RecordModified for enemy-inflicted or otherwise externally-applied changes too. (1) is
    // technically redundant with (2) now but kept as a belt-and-suspenders direct call.
    private static ICombatState? _lastCombat;
    private static readonly List<InvertibleDebuff> _modificationOrder = new();

    public static void RecordModified(Creature creature, InvertibleDebuff debuff)
    {
        var combat = creature.CombatState;
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _modificationOrder.Clear();
        }
        _modificationOrder.Remove(debuff);
        _modificationOrder.Insert(0, debuff);
    }

    private static bool IsPresent(Creature creature, InvertibleDebuff debuff) => debuff switch
    {
        InvertibleDebuff.Weak => creature.GetPowerAmount<WeakPower>() > 0,
        InvertibleDebuff.Vulnerable => creature.GetPowerAmount<VulnerablePower>() > 0,
        InvertibleDebuff.Shaken => creature.GetPowerAmount<ShakenPower>() > 0,
        InvertibleDebuff.Limited => creature.GetPowerAmount<LimitedPower>() > 0,
        InvertibleDebuff.Jaded => creature.GetPowerAmount<JadedPower>() > 0,
        InvertibleDebuff.Frail => creature.GetPowerAmount<FrailPower>() > 0,
        InvertibleDebuff.Strength => creature.GetPowerAmount<StrengthPower>() < 0,
        InvertibleDebuff.Dexterity => creature.GetPowerAmount<DexterityPower>() < 0,
        _ => false
    };

    // All invertible debuffs currently present on a creature, in fixed enum order — the candidate
    // list for Ad Lib's "invert 1 random invertible debuff you currently have" (as opposed to
    // PickDebuffToInvert's "last modified" selection below).
    internal static List<InvertibleDebuff> GetPresentInvertibleDebuffs(Creature creature) =>
        Enum.GetValues<InvertibleDebuff>().Where(d => IsPresent(creature, d)).ToList();

    // Picks which invertible debuff Invert should act on: the most recently modified one that's
    // still actually present (walking back through modification history, since the very latest
    // entry may have already been fully cleared by something else), or — if nothing tracked is
    // present, e.g. tracking missed it or nothing has been modified yet this combat — falls back
    // to any currently-present invertible debuff at all, in fixed enum order. Returns null only
    // if the creature has no invertible debuff whatsoever right now.
    private static InvertibleDebuff? PickDebuffToInvert(Creature creature)
    {
        if (ReferenceEquals(_lastCombat, creature.CombatState))
            foreach (var debuff in _modificationOrder)
                if (IsPresent(creature, debuff)) return debuff;

        foreach (InvertibleDebuff debuff in Enum.GetValues<InvertibleDebuff>())
            if (IsPresent(creature, debuff)) return debuff;

        return null;
    }

    // Invert up to `max` stacks of whichever invertible debuff Invert should currently act on
    // (see PickDebuffToInvert). No-op only if the creature has no invertible debuff at all.
    public static async Task InvertLastModified(PlayerChoiceContext ctx, Creature creature, int max)
    {
        var debuff = PickDebuffToInvert(creature);
        if (debuff == null) return;
        await InvertDebuff(ctx, creature, debuff.Value, max);
    }

    // Invert the last modified invertible debuff (as InvertLastModified), then re-gain that same
    // buff, at the same amount just converted, `repeats` additional times as separate applications
    // — Everything I've Got's "gain the same inverted buff X more times." Separate applications
    // (rather than one lump sum) so once-per-application triggers (e.g. Full Voice) see `repeats`
    // distinct events, matching the Double Time repeat idiom.
    public static async Task InvertLastModifiedWithBonus(PlayerChoiceContext ctx, Creature creature, int invertMax, int repeats)
    {
        var debuff = PickDebuffToInvert(creature);
        if (debuff == null) return;
        int converted = await InvertDebuff(ctx, creature, debuff.Value, invertMax);
        if (converted <= 0) return;
        for (int i = 0; i < repeats; i++)
            await ApplyBuffSide(ctx, creature, debuff.Value, converted);
    }

    // Apply `stacks` of the buff side of one of the 8 invertible pairs (the 6 real Un-X powers, or a
    // positive Strength/Dexterity gain). Used by InvertLastModifiedWithBonus above, and reused by
    // My Own Lesson's InvertTrackerPower swap and Second Lesson's Rewarded resolution.
    public static async Task ApplyBuffSide(PlayerChoiceContext ctx, Creature creature, InvertibleDebuff debuff, int stacks)
    {
        switch (debuff)
        {
            case InvertibleDebuff.Weak:
                await PowerCmd.Apply<UnweakPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Vulnerable:
                await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Shaken:
                await PowerCmd.Apply<UnshakenPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Limited:
                await PowerCmd.Apply<UnlimitedPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Jaded:
                await PowerCmd.Apply<UnjadedPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Frail:
                await PowerCmd.Apply<UnfrailPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Strength:
                await PowerCmd.Apply<StrengthPower>(ctx, creature, stacks, creature, null, false);
                break;
            case InvertibleDebuff.Dexterity:
                await PowerCmd.Apply<DexterityPower>(ctx, creature, stacks, creature, null, false);
                break;
        }
    }

    // Mirror of ApplyBuffSide for the debuff side of one of the 8 invertible pairs (the 6 real X
    // powers, or a negative Strength/Dexterity gain). Reuses the existing per-pair Apply*ToSelf
    // methods above for the 6 real pairs (also recording modification history for Invert).
    public static async Task ApplyDebuffSide(PlayerChoiceContext ctx, Creature creature, InvertibleDebuff debuff, int stacks)
    {
        switch (debuff)
        {
            case InvertibleDebuff.Weak:
                await ApplyWeakToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Vulnerable:
                await ApplyVulnerableToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Shaken:
                await ApplyShakenToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Limited:
                await ApplyLimitedToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Jaded:
                await ApplyJadedToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Frail:
                await ApplyFrailToSelf(ctx, creature, stacks, null);
                break;
            case InvertibleDebuff.Strength:
                await PowerCmd.Apply<StrengthPower>(ctx, creature, -stacks, creature, null, false);
                break;
            case InvertibleDebuff.Dexterity:
                await PowerCmd.Apply<DexterityPower>(ctx, creature, -stacks, creature, null, false);
                break;
        }
    }

    // Invert up to `maxEach` stacks of every invertible debuff the creature currently has any
    // stacks of (Coda's "each invertible debuff you have").
    public static async Task InvertEach(PlayerChoiceContext ctx, Creature creature, int maxEach)
    {
        foreach (InvertibleDebuff debuff in Enum.GetValues<InvertibleDebuff>())
            await InvertDebuff(ctx, creature, debuff, maxEach);
    }

    internal static async Task<int> InvertDebuff(PlayerChoiceContext ctx, Creature creature, InvertibleDebuff debuff, int max) => debuff switch
    {
        InvertibleDebuff.Weak => await ConvertWeakToUnweak(ctx, creature, max),
        InvertibleDebuff.Vulnerable => await ConvertVulnerableToUnvulnerable(ctx, creature, max),
        InvertibleDebuff.Shaken => await ConvertShakenToUnshaken(ctx, creature, max),
        InvertibleDebuff.Limited => await ConvertLimitedToUnlimited(ctx, creature, max),
        InvertibleDebuff.Jaded => await ConvertJadedToUnjaded(ctx, creature, max),
        InvertibleDebuff.Frail => await ConvertFrailToUnfrail(ctx, creature, max),
        InvertibleDebuff.Strength => await InvertStrengthSign(ctx, creature, max),
        InvertibleDebuff.Dexterity => await InvertDexteritySign(ctx, creature, max),
        _ => 0
    };

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

    // Thin, non-pure wrapper built from live creature state for a single pair — Categorize/
    // CategorizeSigned above carry the actual logic and are what's unit-tested.
    public static PairCategory CategorizeForCreature(Creature creature, InvertibleDebuff debuff) => debuff switch
    {
        InvertibleDebuff.Weak => Categorize(creature.GetPowerAmount<WeakPower>(), creature.GetPowerAmount<UnweakPower>()),
        InvertibleDebuff.Vulnerable => Categorize(creature.GetPowerAmount<VulnerablePower>(), creature.GetPowerAmount<UnvulnerablePower>()),
        InvertibleDebuff.Shaken => Categorize(creature.GetPowerAmount<ShakenPower>(), creature.GetPowerAmount<UnshakenPower>()),
        InvertibleDebuff.Limited => Categorize(creature.GetPowerAmount<LimitedPower>(), creature.GetPowerAmount<UnlimitedPower>()),
        InvertibleDebuff.Jaded => Categorize(creature.GetPowerAmount<JadedPower>(), creature.GetPowerAmount<UnjadedPower>()),
        InvertibleDebuff.Frail => Categorize(creature.GetPowerAmount<FrailPower>(), creature.GetPowerAmount<UnfrailPower>()),
        InvertibleDebuff.Strength => CategorizeSigned(creature.GetPowerAmount<StrengthPower>()),
        InvertibleDebuff.Dexterity => CategorizeSigned(creature.GetPowerAmount<DexterityPower>()),
        _ => PairCategory.None
    };

    // Builds the full 8-pair category map for a creature, for Reward's selection (or Punish's,
    // before the First-Lesson exclusion below is applied).
    public static Dictionary<InvertibleDebuff, PairCategory> BuildCategories(Creature creature)
    {
        var result = new Dictionary<InvertibleDebuff, PairCategory>();
        foreach (InvertibleDebuff debuff in Enum.GetValues<InvertibleDebuff>())
            result[debuff] = CategorizeForCreature(creature, debuff);
        return result;
    }

    // While The First Lesson is active, TheFirstLessonPower zeroes any incoming Weak/Vulnerable
    // gain outright, so Punish applying either would be a silent no-op — exclude them from Punish's
    // candidate pool entirely (not just deprioritize) whenever that's the case. Reward's own pool
    // is never filtered this way: The First Lesson only blocks the debuff side, and Unweak/
    // Unvulnerable as buffs are never blocked.
    public static IReadOnlyDictionary<InvertibleDebuff, PairCategory> ExcludeForPunishIfFirstLessonActive(
        IReadOnlyDictionary<InvertibleDebuff, PairCategory> categories, bool firstLessonActive)
    {
        if (!firstLessonActive) return categories;
        var filtered = new Dictionary<InvertibleDebuff, PairCategory>(categories);
        filtered.Remove(InvertibleDebuff.Weak);
        filtered.Remove(InvertibleDebuff.Vulnerable);
        return filtered;
    }

    // Searches priorityOrder in order for the first non-empty category, then hands its candidates
    // to picker (the actual randomness lives outside this pure function, injected as a seam for
    // testing). Given categories always partitions its keys across the 3 PairCategory values, some
    // tier is always non-empty as long as categories itself is non-empty.
    public static InvertibleDebuff PickByPriority(
        IReadOnlyDictionary<InvertibleDebuff, PairCategory> categories,
        IReadOnlyList<PairCategory> priorityOrder,
        Func<IReadOnlyList<InvertibleDebuff>, InvertibleDebuff> picker)
    {
        foreach (var category in priorityOrder)
        {
            var candidates = categories.Where(kv => kv.Value == category).Select(kv => kv.Key).ToList();
            if (candidates.Count > 0) return picker(candidates);
        }
        throw new InvalidOperationException("PickByPriority: categories did not contain any of the given priority tiers.");
    }
}
