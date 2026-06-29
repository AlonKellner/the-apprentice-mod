using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

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

    // Apply Weak to self. Cancels against existing Unweak. Triggers TenacityPower if present.
    public static async Task ApplyWeakToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curWeak = creature.GetPowerAmount<WeakPower>();
        int curUnweak = creature.GetPowerAmount<UnweakPower>();
        var (netWeak, netUnweak) = ComputeNetWeak(curWeak, curUnweak, stacks, 0);
        await AdjustWeakPowers(ctx, creature, card, curWeak, curUnweak, netWeak, netUnweak);

        int tenacityGain = creature.GetPowerAmount<TenacityPower>();
        if (tenacityGain > 0 && stacks > 0)
            await PowerCmd.Apply<VigorPower>(ctx, creature, tenacityGain, creature, card, false);
    }

    // Apply Vulnerable to self. Cancels against existing Unvulnerable. Triggers TenacityPower if present.
    public static async Task ApplyVulnerableToSelf(PlayerChoiceContext ctx, Creature creature, int stacks, CardModel? card)
    {
        int curVul = creature.GetPowerAmount<VulnerablePower>();
        int curUnvul = creature.GetPowerAmount<UnvulnerablePower>();
        var (netVul, netUnvul) = ComputeNetVulnerable(curVul, curUnvul, stacks, 0);
        await AdjustVulnerablePowers(ctx, creature, card, curVul, curUnvul, netVul, netUnvul);

        int tenacityGain = creature.GetPowerAmount<TenacityPower>();
        if (tenacityGain > 0 && stacks > 0)
            await PowerCmd.Apply<VigorPower>(ctx, creature, tenacityGain, creature, card, false);
    }

    // Convert WeakPower to UnweakPower (same stacks, different sign). Max limits stacks converted.
    public static async Task ConvertWeakToUnweak(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int weakAmount = Math.Min(creature.GetPowerAmount<WeakPower>(), max);
        if (weakAmount <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, creature, -weakAmount, creature, null, false);
        await PowerCmd.Apply<UnweakPower>(ctx, creature, weakAmount, creature, null, false);
    }

    // Convert VulnerablePower to UnvulnerablePower (same stacks, different sign). Max limits stacks converted.
    public static async Task ConvertVulnerableToUnvulnerable(PlayerChoiceContext ctx, Creature creature, int max = int.MaxValue)
    {
        int vulAmount = Math.Min(creature.GetPowerAmount<VulnerablePower>(), max);
        if (vulAmount <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, creature, -vulAmount, creature, null, false);
        await PowerCmd.Apply<UnvulnerablePower>(ctx, creature, vulAmount, creature, null, false);
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

    private static async Task AdjustWeakPowers(PlayerChoiceContext ctx, Creature creature, CardModel? card,
        int curWeak, int curUnweak, int netWeak, int netUnweak)
    {
        int weakDelta = netWeak - curWeak;
        int unweakDelta = netUnweak - curUnweak;
        if (weakDelta != 0)
            await PowerCmd.Apply<WeakPower>(ctx, creature, weakDelta, creature, card, false);
        if (unweakDelta != 0)
            await PowerCmd.Apply<UnweakPower>(ctx, creature, unweakDelta, creature, card, false);
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
    }
}
