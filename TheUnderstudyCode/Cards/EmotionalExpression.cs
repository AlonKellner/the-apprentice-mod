using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

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

    // Convert up to max WeakPower to UnweakPower. The raw removeAmount (not a pre-reduced value)
    // is what's granted to Unweak — InvertTrackerPower's interception (canonicalPower is
    // UnweakPower) reduces that raw amount by whatever Weak remains after the removal above, live,
    // at the moment this call (and each Fortissimo repeat of it) actually lands. This is what makes
    // the amount of cancellation depend on how much debuff is left over after a capped Invert,
    // instead of a single precomputed net.
    public static async Task ConvertWeakToUnweak(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        int removeAmount = Math.Min(curWeak, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnweakPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Weak);
        await RaiseClearedIfZeroed(ctx, creature, curWeak, creature.GetPowerAmount<WeakPower>());
    }

    // Mirror of ConvertWeakToUnweak for Vulnerable/Unvulnerable.
    public static async Task ConvertVulnerableToUnvulnerable(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        int removeAmount = Math.Min(curVul, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Vulnerable);
        await RaiseClearedIfZeroed(ctx, creature, curVul, creature.GetPowerAmount<VulnerablePower>());
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

    // Invoked whenever one of the 5 self-debuffs transitions from present (>0) to fully cleared (0)
    // as a direct result of one of this class's own Invert calls — e.g. Take Notes' "whenever
    // a debuff of yours clears, gain Vigor." A Func rather than a true multicast event since it
    // needs to be awaited with the live PlayerChoiceContext (only one subscriber is expected at a
    // time in practice). Known limitation, same shape as the "last modified" tracker: only
    // observes this class's own operations, not natural per-turn decay or anything enemy-inflicted.
    public static Func<PlayerChoiceContext, Creature, Task>? DebuffCleared;

    private static async Task RaiseClearedIfZeroed(PlayerChoiceContext ctx, Creature creature, int curDebuff, int netDebuff)
    {
        if (curDebuff > 0 && netDebuff == 0 && DebuffCleared != null)
            await DebuffCleared(ctx, creature);
    }

    // Shaken/Unshaken — same shape as Weak/Unweak.

    public static async Task ApplyShakenToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<ShakenPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Shaken);
    }

    public static async Task ConvertShakenToUnshaken(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curShaken = creature.GetPowerAmount<ShakenPower>();
        int removeAmount = Math.Min(curShaken, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<ShakenPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnshakenPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Shaken);
        await RaiseClearedIfZeroed(ctx, creature, curShaken, creature.GetPowerAmount<ShakenPower>());
    }

    // Limited/Unlimited — same shape as Weak/Unweak, throttling draw instead of dealt damage.

    public static async Task ApplyLimitedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Limited);
    }

    public static async Task ConvertLimitedToUnlimited(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curLimited = creature.GetPowerAmount<LimitedPower>();
        int removeAmount = Math.Min(curLimited, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<LimitedPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnlimitedPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Limited);
        await RaiseClearedIfZeroed(ctx, creature, curLimited, creature.GetPowerAmount<LimitedPower>());
    }

    // Jaded/Unjaded — same shape as Limited/Unlimited, throttling next turn's Energy instead of draw.

    public static async Task ApplyJadedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        if (stacks <= 0) return;
        await PowerCmd.Apply<JadedPower>(ctx, creature, stacks, creature, card, false);
        RecordModified(creature, InvertibleDebuff.Jaded);
    }

    public static async Task ConvertJadedToUnjaded(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curJaded = creature.GetPowerAmount<JadedPower>();
        int removeAmount = Math.Min(curJaded, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<JadedPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnjadedPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Jaded);
        await RaiseClearedIfZeroed(ctx, creature, curJaded, creature.GetPowerAmount<JadedPower>());
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

    public static async Task ConvertFrailToUnfrail(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curFrail = creature.GetPowerAmount<FrailPower>();
        int removeAmount = Math.Min(curFrail, max);
        if (removeAmount <= 0) return;
        await PowerCmd.Apply<FrailPower>(ctx, creature, -removeAmount, creature, null, false);
        await PowerCmd.Apply<UnfrailPower>(ctx, creature, removeAmount, creature, null, false);
        RecordModified(creature, InvertibleDebuff.Frail);
        await RaiseClearedIfZeroed(ctx, creature, curFrail, creature.GetPowerAmount<FrailPower>());
    }

    // Sum of the 5 self-debuff flavors currently on a creature — the scaling metric used by
    // "Project"-style cards (Apply [Debuff] equal to the sum of your invertible debuffs).
    // Deliberately only the 5 flavors the Understudy's own cards generate (Weak, Vulnerable,
    // Shaken, Limited, Jaded) — Frail/Strength/Dexterity are Invert-recognized but never part of
    // this sum, matching how the mechanic has been scoped throughout design.
    public static int SumOfInvertibleDebuffs(Creature creature) =>
        creature.GetPowerAmount<WeakPower>()
        + creature.GetPowerAmount<VulnerablePower>()
        + creature.GetPowerAmount<ShakenPower>()
        + creature.GetPowerAmount<LimitedPower>()
        + creature.GetPowerAmount<JadedPower>();

    // ── Invert dispatcher ────────────────────────────────────────────────────────────────────
    //
    // "Last modified invertible debuff" tracking, combat-scoped like IntenseModifier's own static
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

    // Invert the last modified invertible debuff (as InvertLastModified), then grant `bonus`
    // additional stacks of whichever buff was just gained by that inversion — Everything I've
    // Got's "gain X more stacks of the buff gained this way."
    public static async Task InvertLastModifiedWithBonus(PlayerChoiceContext ctx, Creature creature, int invertMax, int bonus)
    {
        var debuff = PickDebuffToInvert(creature);
        if (debuff == null) return;
        await InvertDebuff(ctx, creature, debuff.Value, invertMax);
        if (bonus > 0) await GrantBonusBuff(ctx, creature, debuff.Value, bonus);
    }

    private static async Task GrantBonusBuff(PlayerChoiceContext ctx, Creature creature, InvertibleDebuff debuff, int bonus)
    {
        switch (debuff)
        {
            case InvertibleDebuff.Weak:
                await PowerCmd.Apply<UnweakPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Vulnerable:
                await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Shaken:
                await PowerCmd.Apply<UnshakenPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Limited:
                await PowerCmd.Apply<UnlimitedPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Jaded:
                await PowerCmd.Apply<UnjadedPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Frail:
                await PowerCmd.Apply<UnfrailPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Strength:
                await PowerCmd.Apply<StrengthPower>(ctx, creature, bonus, creature, null, false);
                break;
            case InvertibleDebuff.Dexterity:
                await PowerCmd.Apply<DexterityPower>(ctx, creature, bonus, creature, null, false);
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

    private static async Task InvertDebuff(PlayerChoiceContext ctx, Creature creature, InvertibleDebuff debuff, int max)
    {
        switch (debuff)
        {
            case InvertibleDebuff.Weak:
                await ConvertWeakToUnweak(ctx, creature, max);
                break;
            case InvertibleDebuff.Vulnerable:
                await ConvertVulnerableToUnvulnerable(ctx, creature, max);
                break;
            case InvertibleDebuff.Shaken:
                await ConvertShakenToUnshaken(ctx, creature, max);
                break;
            case InvertibleDebuff.Limited:
                await ConvertLimitedToUnlimited(ctx, creature, max);
                break;
            case InvertibleDebuff.Jaded:
                await ConvertJadedToUnjaded(ctx, creature, max);
                break;
            case InvertibleDebuff.Frail:
                await ConvertFrailToUnfrail(ctx, creature, max);
                break;
            case InvertibleDebuff.Strength:
                await InvertStrengthSign(ctx, creature, max);
                break;
            case InvertibleDebuff.Dexterity:
                await InvertDexteritySign(ctx, creature, max);
                break;
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

    public static async Task InvertStrengthSign(PlayerChoiceContext ctx, Creature creature, int max)
    {
        int cur = creature.GetPowerAmount<StrengthPower>();
        var (converted, _) = ComputeSignFlip(cur, max);
        if (converted <= 0) return;
        await PowerCmd.Apply<StrengthPower>(ctx, creature, 2 * converted, creature, null, false);
    }

    public static async Task InvertDexteritySign(PlayerChoiceContext ctx, Creature creature, int max)
    {
        int cur = creature.GetPowerAmount<DexterityPower>();
        var (converted, _) = ComputeSignFlip(cur, max);
        if (converted <= 0) return;
        await PowerCmd.Apply<DexterityPower>(ctx, creature, 2 * converted, creature, null, false);
    }
}
