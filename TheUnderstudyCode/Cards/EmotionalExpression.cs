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
    // Pure math — no game state required; testable in isolation.

    public static (int netWeak, int netUnweak) ComputeNetWeak(int curWeak, int curUnweak, int deltaWeak, int deltaUnweak)
    {
        int totalWeak = curWeak + deltaWeak;
        int totalUnweak = curUnweak + deltaUnweak;
        if (totalWeak >= totalUnweak)
            return (totalWeak - totalUnweak, 0);
        return (0, totalUnweak - totalWeak);
    }

    public static int CountUniqueDebuffTypes(int weakAmt, int vulAmt) =>
        (weakAmt > 0 ? 1 : 0) + (vulAmt > 0 ? 1 : 0);

    public static (int reducedAmount, int consumed) ComputeWeakCancellation(int weakApplied, int unweakAvailable)
    {
        int consumed = Math.Min(weakApplied, unweakAvailable);
        return (weakApplied - consumed, consumed);
    }

    public static (int netVul, int netUnvul) ComputeNetVulnerable(int curVul, int curUnvul, int deltaVul, int deltaUnvul)
    {
        int totalVul = curVul + deltaVul;
        int totalUnvul = curUnvul + deltaUnvul;
        if (totalVul >= totalUnvul)
            return (totalVul - totalUnvul, 0);
        return (0, totalUnvul - totalVul);
    }

    // "Convert up to max Weak to Unweak." A capped conversion can leave Weak remaining alongside
    // the newly-created Unweak — those must cancel against each other (Weak/Unweak are mutually
    // exclusive by design) rather than coexist, so this routes through ComputeNetWeak.
    public static (int netWeak, int netUnweak) ComputeWeakConversion(int curWeak, int curUnweak, int max)
    {
        int weakAmount = Math.Min(curWeak, max);
        if (weakAmount <= 0) return (curWeak, curUnweak);
        return ComputeNetWeak(curWeak, curUnweak, -weakAmount, weakAmount);
    }

    // Mirror of ComputeWeakConversion for Vulnerable/Unvulnerable.
    public static (int netVul, int netUnvul) ComputeVulnerableConversion(int curVul, int curUnvul, int max)
    {
        int vulAmount = Math.Min(curVul, max);
        if (vulAmount <= 0) return (curVul, curUnvul);
        return ComputeNetVulnerable(curVul, curUnvul, -vulAmount, vulAmount);
    }

    // Apply Unweak to a creature, cancelling against any existing WeakPower.
    public static async Task ApplyUnweak(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        int curUnweak = creature.GetPowerAmount<UnweakPower>();
        var (netWeak, netUnweak) = ComputeNetWeak(curWeak, curUnweak, 0, stacks);
        await AdjustWeakPowers(ctx, creature, card, curWeak, curUnweak, netWeak, netUnweak);
    }

    // Apply Unvulnerable to a creature, cancelling against any existing VulnerablePower.
    public static async Task ApplyUnvulnerable(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        int curUnvul = creature.GetPowerAmount<UnvulnerablePower>();
        var (netVul, netUnvul) = ComputeNetVulnerable(curVul, curUnvul, 0, stacks);
        await AdjustVulnerablePowers(ctx, creature, card, curVul, curUnvul, netVul, netUnvul);
    }

    // Apply Weak to self. Cancels against existing Unweak.
    public static async Task ApplyWeakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        int curUnweak = creature.GetPowerAmount<UnweakPower>();
        var (netWeak, netUnweak) = ComputeNetWeak(curWeak, curUnweak, stacks, 0);
        await AdjustWeakPowers(ctx, creature, card, curWeak, curUnweak, netWeak, netUnweak);
    }

    // Apply Vulnerable to self. Cancels against existing Unvulnerable.
    public static async Task ApplyVulnerableToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        int curUnvul = creature.GetPowerAmount<UnvulnerablePower>();
        var (netVul, netUnvul) = ComputeNetVulnerable(curVul, curUnvul, stacks, 0);
        await AdjustVulnerablePowers(ctx, creature, card, curVul, curUnvul, netVul, netUnvul);
    }

    // Convert up to max WeakPower to UnweakPower. Any remaining Weak (when current Weak exceeds
    // max) cancels against the newly-created Unweak via ComputeWeakConversion, so the two never
    // coexist.
    public static async Task ConvertWeakToUnweak(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        if (curWeak <= 0 || max <= 0) return;
        int curUnweak = creature.GetPowerAmount<UnweakPower>();
        var (netWeak, netUnweak) = ComputeWeakConversion(curWeak, curUnweak, max);
        await AdjustWeakPowers(ctx, creature, null, curWeak, curUnweak, netWeak, netUnweak);
    }

    // Mirror of ConvertWeakToUnweak for Vulnerable/Unvulnerable.
    public static async Task ConvertVulnerableToUnvulnerable(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        if (curVul <= 0 || max <= 0) return;
        int curUnvul = creature.GetPowerAmount<UnvulnerablePower>();
        var (netVul, netUnvul) = ComputeVulnerableConversion(curVul, curUnvul, max);
        await AdjustVulnerablePowers(ctx, creature, null, curVul, curUnvul, netVul, netUnvul);
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
    // as a direct result of one of this class's own Apply/Invert calls — e.g. Take Notes' "whenever
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

    private static async Task AdjustWeakPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curWeak, int curUnweak, int netWeak, int netUnweak)
    {
        int weakDelta = netWeak - curWeak;
        int unweakDelta = netUnweak - curUnweak;
        if (weakDelta != 0)
            await PowerCmd.Apply<WeakPower>(ctx, creature, weakDelta, creature, card, false);
        if (unweakDelta != 0)
            await PowerCmd.Apply<UnweakPower>(ctx, creature, unweakDelta, creature, card, false);
        if (weakDelta != 0 || unweakDelta != 0)
            RecordModified(creature, InvertibleDebuff.Weak);
        await RaiseClearedIfZeroed(ctx, creature, curWeak, netWeak);
    }

    private static async Task AdjustVulnerablePowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curVul, int curUnvul, int netVul, int netUnvul)
    {
        int vulDelta = netVul - curVul;
        int unvulDelta = netUnvul - curUnvul;
        if (vulDelta != 0)
            await PowerCmd.Apply<VulnerablePower>(ctx, creature, vulDelta, creature, card, false);
        if (unvulDelta != 0)
            await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, unvulDelta, creature, card, false);
        if (vulDelta != 0 || unvulDelta != 0)
            RecordModified(creature, InvertibleDebuff.Vulnerable);
        await RaiseClearedIfZeroed(ctx, creature, curVul, netVul);
    }

    public static (int netShaken, int netUnshaken) ComputeNetShaken(int curShaken, int curUnshaken, int deltaShaken, int deltaUnshaken)
    {
        int totalShaken = curShaken + deltaShaken;
        int totalUnshaken = curUnshaken + deltaUnshaken;
        if (totalShaken >= totalUnshaken)
            return (totalShaken - totalUnshaken, 0);
        return (0, totalUnshaken - totalShaken);
    }

    public static (int netShaken, int netUnshaken) ComputeShakenConversion(int curShaken, int curUnshaken, int max)
    {
        int shakenAmount = Math.Min(curShaken, max);
        if (shakenAmount <= 0) return (curShaken, curUnshaken);
        return ComputeNetShaken(curShaken, curUnshaken, -shakenAmount, shakenAmount);
    }

    public static async Task ApplyShakenToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curShaken = creature.GetPowerAmount<ShakenPower>();
        int curUnshaken = creature.GetPowerAmount<UnshakenPower>();
        var (netShaken, netUnshaken) = ComputeNetShaken(curShaken, curUnshaken, stacks, 0);
        await AdjustShakenPowers(ctx, creature, card, curShaken, curUnshaken, netShaken, netUnshaken);
    }

    public static async Task ConvertShakenToUnshaken(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curShaken = creature.GetPowerAmount<ShakenPower>();
        if (curShaken <= 0 || max <= 0) return;
        int curUnshaken = creature.GetPowerAmount<UnshakenPower>();
        var (netShaken, netUnshaken) = ComputeShakenConversion(curShaken, curUnshaken, max);
        await AdjustShakenPowers(ctx, creature, null, curShaken, curUnshaken, netShaken, netUnshaken);
    }

    private static async Task AdjustShakenPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curShaken, int curUnshaken, int netShaken, int netUnshaken)
    {
        int shakenDelta = netShaken - curShaken;
        int unshakenDelta = netUnshaken - curUnshaken;
        if (shakenDelta != 0)
            await PowerCmd.Apply<ShakenPower>(ctx, creature, shakenDelta, creature, card, false);
        if (unshakenDelta != 0)
            await PowerCmd.Apply<UnshakenPower>(ctx, creature, unshakenDelta, creature, card, false);
        if (shakenDelta != 0 || unshakenDelta != 0)
            RecordModified(creature, InvertibleDebuff.Shaken);
        await RaiseClearedIfZeroed(ctx, creature, curShaken, netShaken);
    }

    public static (int netLimited, int netUnlimited) ComputeNetLimited(int curLimited, int curUnlimited, int deltaLimited, int deltaUnlimited)
    {
        int totalLimited = curLimited + deltaLimited;
        int totalUnlimited = curUnlimited + deltaUnlimited;
        if (totalLimited >= totalUnlimited)
            return (totalLimited - totalUnlimited, 0);
        return (0, totalUnlimited - totalLimited);
    }

    public static (int netLimited, int netUnlimited) ComputeLimitedConversion(int curLimited, int curUnlimited, int max)
    {
        int limitedAmount = Math.Min(curLimited, max);
        if (limitedAmount <= 0) return (curLimited, curUnlimited);
        return ComputeNetLimited(curLimited, curUnlimited, -limitedAmount, limitedAmount);
    }

    // Apply Limited to self. Cancels against existing Unlimited.
    public static async Task ApplyLimitedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curLimited = creature.GetPowerAmount<LimitedPower>();
        int curUnlimited = creature.GetPowerAmount<UnlimitedPower>();
        var (netLimited, netUnlimited) = ComputeNetLimited(curLimited, curUnlimited, stacks, 0);
        await AdjustLimitedPowers(ctx, creature, card, curLimited, curUnlimited, netLimited, netUnlimited);
    }

    public static async Task ConvertLimitedToUnlimited(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curLimited = creature.GetPowerAmount<LimitedPower>();
        if (curLimited <= 0 || max <= 0) return;
        int curUnlimited = creature.GetPowerAmount<UnlimitedPower>();
        var (netLimited, netUnlimited) = ComputeLimitedConversion(curLimited, curUnlimited, max);
        await AdjustLimitedPowers(ctx, creature, null, curLimited, curUnlimited, netLimited, netUnlimited);
    }

    private static async Task AdjustLimitedPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curLimited, int curUnlimited, int netLimited, int netUnlimited)
    {
        int limitedDelta = netLimited - curLimited;
        int unlimitedDelta = netUnlimited - curUnlimited;
        if (limitedDelta != 0)
            await PowerCmd.Apply<LimitedPower>(ctx, creature, limitedDelta, creature, card, false);
        if (unlimitedDelta != 0)
            await PowerCmd.Apply<UnlimitedPower>(ctx, creature, unlimitedDelta, creature, card, false);
        if (limitedDelta != 0 || unlimitedDelta != 0)
            RecordModified(creature, InvertibleDebuff.Limited);
        await RaiseClearedIfZeroed(ctx, creature, curLimited, netLimited);
    }

    // Jaded/Unjaded — same shape as Limited/Unlimited, throttling next turn's Energy instead of draw.

    public static (int netJaded, int netUnjaded) ComputeNetJaded(int curJaded, int curUnjaded, int deltaJaded, int deltaUnjaded)
    {
        int totalJaded = curJaded + deltaJaded;
        int totalUnjaded = curUnjaded + deltaUnjaded;
        if (totalJaded >= totalUnjaded)
            return (totalJaded - totalUnjaded, 0);
        return (0, totalUnjaded - totalJaded);
    }

    public static (int netJaded, int netUnjaded) ComputeJadedConversion(int curJaded, int curUnjaded, int max)
    {
        int jadedAmount = Math.Min(curJaded, max);
        if (jadedAmount <= 0) return (curJaded, curUnjaded);
        return ComputeNetJaded(curJaded, curUnjaded, -jadedAmount, jadedAmount);
    }

    // Apply Jaded to self. Cancels against existing Unjaded.
    public static async Task ApplyJadedToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curJaded = creature.GetPowerAmount<JadedPower>();
        int curUnjaded = creature.GetPowerAmount<UnjadedPower>();
        var (netJaded, netUnjaded) = ComputeNetJaded(curJaded, curUnjaded, stacks, 0);
        await AdjustJadedPowers(ctx, creature, card, curJaded, curUnjaded, netJaded, netUnjaded);
    }

    public static async Task ConvertJadedToUnjaded(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curJaded = creature.GetPowerAmount<JadedPower>();
        if (curJaded <= 0 || max <= 0) return;
        int curUnjaded = creature.GetPowerAmount<UnjadedPower>();
        var (netJaded, netUnjaded) = ComputeJadedConversion(curJaded, curUnjaded, max);
        await AdjustJadedPowers(ctx, creature, null, curJaded, curUnjaded, netJaded, netUnjaded);
    }

    private static async Task AdjustJadedPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curJaded, int curUnjaded, int netJaded, int netUnjaded)
    {
        int jadedDelta = netJaded - curJaded;
        int unjadedDelta = netUnjaded - curUnjaded;
        if (jadedDelta != 0)
            await PowerCmd.Apply<JadedPower>(ctx, creature, jadedDelta, creature, card, false);
        if (unjadedDelta != 0)
            await PowerCmd.Apply<UnjadedPower>(ctx, creature, unjadedDelta, creature, card, false);
        if (jadedDelta != 0 || unjadedDelta != 0)
            RecordModified(creature, InvertibleDebuff.Jaded);
        await RaiseClearedIfZeroed(ctx, creature, curJaded, netJaded);
    }

    // Frail/Unfrail — same shape as Weak/Unweak, reducing Block gain instead of dealt damage.
    // No Understudy card applies Frail directly; this exists purely so Invert (and the
    // Invert-each variant on Coda) behaves correctly if something external inflicts it.

    public static (int netFrail, int netUnfrail) ComputeNetFrail(int curFrail, int curUnfrail, int deltaFrail, int deltaUnfrail)
    {
        int totalFrail = curFrail + deltaFrail;
        int totalUnfrail = curUnfrail + deltaUnfrail;
        if (totalFrail >= totalUnfrail)
            return (totalFrail - totalUnfrail, 0);
        return (0, totalUnfrail - totalFrail);
    }

    public static (int netFrail, int netUnfrail) ComputeFrailConversion(int curFrail, int curUnfrail, int max)
    {
        int frailAmount = Math.Min(curFrail, max);
        if (frailAmount <= 0) return (curFrail, curUnfrail);
        return ComputeNetFrail(curFrail, curUnfrail, -frailAmount, frailAmount);
    }

    // Apply Frail to self. Cancels against existing Unfrail.
    public static async Task ApplyFrailToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curFrail = creature.GetPowerAmount<FrailPower>();
        int curUnfrail = creature.GetPowerAmount<UnfrailPower>();
        var (netFrail, netUnfrail) = ComputeNetFrail(curFrail, curUnfrail, stacks, 0);
        await AdjustFrailPowers(ctx, creature, card, curFrail, curUnfrail, netFrail, netUnfrail);
    }

    public static async Task ConvertFrailToUnfrail(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int curFrail = creature.GetPowerAmount<FrailPower>();
        if (curFrail <= 0 || max <= 0) return;
        int curUnfrail = creature.GetPowerAmount<UnfrailPower>();
        var (netFrail, netUnfrail) = ComputeFrailConversion(curFrail, curUnfrail, max);
        await AdjustFrailPowers(ctx, creature, null, curFrail, curUnfrail, netFrail, netUnfrail);
    }

    private static async Task AdjustFrailPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curFrail, int curUnfrail, int netFrail, int netUnfrail)
    {
        int frailDelta = netFrail - curFrail;
        int unfrailDelta = netUnfrail - curUnfrail;
        if (frailDelta != 0)
            await PowerCmd.Apply<FrailPower>(ctx, creature, frailDelta, creature, card, false);
        if (unfrailDelta != 0)
            await PowerCmd.Apply<UnfrailPower>(ctx, creature, unfrailDelta, creature, card, false);
        if (frailDelta != 0 || unfrailDelta != 0)
            RecordModified(creature, InvertibleDebuff.Frail);
        await RaiseClearedIfZeroed(ctx, creature, curFrail, netFrail);
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
    // counter. Updated by every Adjust*Powers method above whenever it actually changes something,
    // covering every Understudy card's own self-application/inversion. Known limitation: this does
    // NOT observe debuffs inflicted by an enemy or another mod directly on the corresponding base
    // Power classes (WeakPower/VulnerablePower are base-game types we can't add hooks to) — only
    // this deck's own Apply/Convert calls update the tracker. Acceptable for now; broadening this
    // to a fully general "any source" observer would need a dedicated always-attached Power
    // overriding AfterModifyingPowerAmountReceived, which is a larger lift than this pass covers.
    private static ICombatState? _lastCombat;
    private static InvertibleDebuff? _lastModifiedDebuff;

    private static void RecordModified(Creature creature, InvertibleDebuff debuff)
    {
        var combat = creature.CombatState;
        if (!ReferenceEquals(combat, _lastCombat))
            _lastCombat = combat;
        _lastModifiedDebuff = debuff;
    }

    // Invert up to `max` stacks of whichever invertible debuff was last modified (by any of this
    // class's own Apply/Convert calls) in the current combat. No-op if nothing has been modified
    // yet, or if tracked state belongs to a different (e.g. already-ended) combat.
    public static async Task InvertLastModified(PlayerChoiceContext ctx, Creature creature, int max)
    {
        if (_lastModifiedDebuff == null || !ReferenceEquals(_lastCombat, creature.CombatState)) return;
        await InvertDebuff(ctx, creature, _lastModifiedDebuff.Value, max);
    }

    // Invert the last modified invertible debuff (as InvertLastModified), then grant `bonus`
    // additional stacks of whichever buff was just gained by that inversion — Everything I've
    // Got's "gain X more stacks of the buff gained this way."
    public static async Task InvertLastModifiedWithBonus(PlayerChoiceContext ctx, Creature creature, int invertMax, int bonus)
    {
        if (_lastModifiedDebuff == null || !ReferenceEquals(_lastCombat, creature.CombatState)) return;
        var debuff = _lastModifiedDebuff.Value;
        await InvertDebuff(ctx, creature, debuff, invertMax);
        if (bonus > 0) await GrantBonusBuff(ctx, creature, debuff, bonus);
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
    // Unlike the 5 pairs above, Strength/Dexterity have no separate "Un-" Power — only negative
    // stacks on the same Power convert, positive stacks are untouched. Confirmed formula: for
    // Invert N against a Power currently at value V, convert = min(N, max(0, -V)),
    // newValue = V + 2*convert (each converted stack removes 1 negative and adds 1 positive).
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
