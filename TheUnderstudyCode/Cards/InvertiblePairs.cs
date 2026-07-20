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

// THE single source of truth for every invertible buff/debuff pair. Add a pair to `All` (and its power
// class if new) and it flows automatically into: Invert (EmotionalExpression.InvertEach), The Second
// Lesson's Reward/Punish pool (SecondLessonPower), InvertTrackerPower's bidirectional net-cancellation, and
// DoubleTimePower.IsInvertiblePower (hence the tooltip "Invertible" tag). No per-consumer edits needed.
public abstract class InvertiblePair
{
    public abstract string Name { get; }   // readable id, for logs/diagnostics
    public abstract bool Contains(PowerModel power);
    public abstract bool IsSameShape { get; }              // X/Un-X (two powers) vs sign-flip (one power)
    public abstract bool IsWeakOrVulnerable { get; }       // The First Lesson excludes these from Punish
    public abstract EmotionalExpression.PairCategory Categorize(Creature c);
    public abstract bool HasDebuffPresent(Creature c);
    public abstract Task ApplyBuffSide(PlayerChoiceContext ctx, Creature c, int stacks);
    public abstract Task ApplyDebuffSide(PlayerChoiceContext ctx, Creature c, int stacks);
    public abstract Task<int> Invert(PlayerChoiceContext ctx, Creature c, int max);  // debuff -> buff, returns converted

    // Give & Take's per-pair resolution: computes Swap's give and Invert's flip from the SAME snapshot of
    // this pair's debuff, removes the consumed debuff from `self` once (capped, never below zero), then
    // grants the inverted buff to `self` and applies the given debuff to every enemy — so "Swap & Invert
    // simultaneously" both act on the original amount instead of Swap consuming what Invert needed.
    public abstract Task ResolveGiveAndTake(
        PlayerChoiceContext ctx, Creature self, IReadOnlyList<Creature> enemies, int swapCap, int invertMax);

    // Same-shape net-cancellation support, used by InvertTrackerPower. `IsDebuffSide` says which side a power
    // is; `OpposingStock` is the current stock of the OTHER side of the pair (0 for sign-flip, which self-nets).
    public abstract bool IsDebuffSide(PowerModel power);
    public abstract int OpposingStock(Creature c, PowerModel gainedSide);
    public abstract Task DecrementOpposing(Creature c, PowerModel gainedSide, int count);
}

// X/Un-X pair, e.g. Weak/Unweak, Tainted/Untainted, Doom/Undoom — generic over the two power types.
public sealed class SameShapePair<TDebuff, TBuff> : InvertiblePair
    where TDebuff : PowerModel
    where TBuff : PowerModel
{
    public override string Name => typeof(TDebuff).Name.Replace("Power", "");
    public override bool IsSameShape => true;
    public override bool IsWeakOrVulnerable => typeof(TDebuff) == typeof(WeakPower) || typeof(TDebuff) == typeof(VulnerablePower);

    public override bool Contains(PowerModel power) => power is TDebuff or TBuff;
    public override bool IsDebuffSide(PowerModel power) => power is TDebuff;

    public override EmotionalExpression.PairCategory Categorize(Creature c) =>
        EmotionalExpression.Categorize(c.GetPowerAmount<TDebuff>(), c.GetPowerAmount<TBuff>());
    public override bool HasDebuffPresent(Creature c) => c.GetPowerAmount<TDebuff>() > 0;

    public override Task ApplyBuffSide(PlayerChoiceContext ctx, Creature c, int stacks) =>
        stacks <= 0 ? Task.CompletedTask : PowerCmd.Apply<TBuff>(ctx, c, stacks, c, null, false);
    public override Task ApplyDebuffSide(PlayerChoiceContext ctx, Creature c, int stacks) =>
        stacks <= 0 ? Task.CompletedTask : PowerCmd.Apply<TDebuff>(ctx, c, stacks, c, null, false);

    // Remove up to `max` of the debuff and grant that raw amount to the buff. The buff grant is netted live
    // by InvertTrackerPower against any leftover debuff (matching the old per-pair ConvertXToUnX helpers).
    public override async Task<int> Invert(PlayerChoiceContext ctx, Creature c, int max)
    {
        int removeAmount = Math.Min(c.GetPowerAmount<TDebuff>(), max);
        if (removeAmount <= 0) return 0;
        await PowerCmd.Apply<TDebuff>(ctx, c, -removeAmount, c, null, false);
        await PowerCmd.Apply<TBuff>(ctx, c, removeAmount, c, null, false);
        return removeAmount;
    }

    public override async Task ResolveGiveAndTake(
        PlayerChoiceContext ctx, Creature self, IReadOnlyList<Creature> enemies, int swapCap, int invertMax)
    {
        int amount = self.GetPowerAmount<TDebuff>();
        if (amount <= 0) return;

        // Only debuffs in the Swap registry are pushed onto enemies; the rest are invert-only.
        bool swappable = SceneStealing.IsSwappableEntry(ModelDb.Power<TDebuff>().Id.Entry);
        var (swapRemove, invertRemove, selfRemove) =
            InvertiblePairs.ComputeGiveAndTake(amount, swappable ? swapCap : 0, invertMax);

        // Remove the consumed debuff from yourself FIRST, so the Un-X buff you gain next isn't live-netted
        // against the very debuff it was inverted from (InvertTrackerPower cancels opposing sides).
        if (selfRemove > 0) await PowerCmd.Apply<TDebuff>(ctx, self, -selfRemove, self, null, false);
        if (invertRemove > 0) await PowerCmd.Apply<TBuff>(ctx, self, invertRemove, self, null, false);
        foreach (var enemy in enemies)
            if (swapRemove > 0) await PowerCmd.Apply<TDebuff>(ctx, enemy, swapRemove, self, null, false);
    }

    public override int OpposingStock(Creature c, PowerModel gainedSide) =>
        gainedSide is TDebuff ? c.GetPowerAmount<TBuff>() : c.GetPowerAmount<TDebuff>();

    public override async Task DecrementOpposing(Creature c, PowerModel gainedSide, int count)
    {
        var opposing = gainedSide is TDebuff ? c.GetPower<TBuff>() : (PowerModel?)c.GetPower<TDebuff>();
        for (int i = 0; i < count && opposing != null; i++)
            await PowerCmd.Decrement(opposing);
    }
}

// Sign-flip pair (Strength/Dexterity/Vigor): one power, positive = buff, negative = debuff. Invert flips the
// negative portion to positive (V -> V + 2*converted). No separate Un-X power, so nothing to net-cancel.
public sealed class SignFlipPair<TPower> : InvertiblePair where TPower : PowerModel
{
    public override string Name => typeof(TPower).Name.Replace("Power", "");
    public override bool IsSameShape => false;
    public override bool IsWeakOrVulnerable => false;

    public override bool Contains(PowerModel power) => power is TPower;
    public override bool IsDebuffSide(PowerModel power) => false;   // sign encodes the side on one power

    public override EmotionalExpression.PairCategory Categorize(Creature c) =>
        EmotionalExpression.CategorizeSigned(c.GetPowerAmount<TPower>());
    public override bool HasDebuffPresent(Creature c) => c.GetPowerAmount<TPower>() < 0;

    public override Task ApplyBuffSide(PlayerChoiceContext ctx, Creature c, int stacks) =>
        stacks == 0 ? Task.CompletedTask : PowerCmd.Apply<TPower>(ctx, c, stacks, c, null, false);
    public override Task ApplyDebuffSide(PlayerChoiceContext ctx, Creature c, int stacks) =>
        stacks == 0 ? Task.CompletedTask : PowerCmd.Apply<TPower>(ctx, c, -stacks, c, null, false);

    public override async Task<int> Invert(PlayerChoiceContext ctx, Creature c, int max)
    {
        var (converted, _) = EmotionalExpression.ComputeSignFlip(c.GetPowerAmount<TPower>(), max);
        if (converted <= 0) return 0;
        await PowerCmd.Apply<TPower>(ctx, c, 2 * converted, c, null, false);
        return converted;
    }

    // Sign-flip debuffs (negative Vigor/Strength/Dexterity) are inverted on yourself only — not pushed onto
    // enemies. Give-to-enemy + flip-on-self + removal don't compose cleanly on a single signed power, and a
    // player holding negative Vigor/Strength/Dexterity when playing this is a rare edge.
    public override Task ResolveGiveAndTake(
        PlayerChoiceContext ctx, Creature self, IReadOnlyList<Creature> enemies, int swapCap, int invertMax) =>
        Invert(ctx, self, invertMax);

    public override int OpposingStock(Creature c, PowerModel gainedSide) => 0;
    public override Task DecrementOpposing(Creature c, PowerModel gainedSide, int count) => Task.CompletedTask;
}

public static class InvertiblePairs
{
    public static readonly IReadOnlyList<InvertiblePair> All = new InvertiblePair[]
    {
        new SameShapePair<WeakPower, UnweakPower>(),
        new SameShapePair<VulnerablePower, UnvulnerablePower>(),
        new SameShapePair<ShakenPower, UnshakenPower>(),
        new SameShapePair<LimitedPower, UnlimitedPower>(),
        new SameShapePair<JadedPower, UnjadedPower>(),
        new SameShapePair<FrailPower, UnfrailPower>(),
        new SameShapePair<TaintedPower, UntaintedPower>(),
        new SameShapePair<TensionPower, UntensionPower>(),
        new SameShapePair<DoomPower, UndoomPower>(),
        new SignFlipPair<StrengthPower>(),
        new SignFlipPair<DexterityPower>(),
        new SignFlipPair<VigorPower>(),
    };

    public static InvertiblePair? For(PowerModel power) => All.FirstOrDefault(p => p.Contains(power));

    // Pure Give & Take math for one debuff of magnitude `amount`: how much Swap gives to each enemy
    // (capped by `swapCap`), how much Invert flips into a buff on you (capped by `invertMax`), and how much
    // total to strip from yourself — both operations read `amount` (the snapshot), and the self-removal is
    // capped at `amount` so it can never go below zero even when swap+invert together exceed what you hold.
    public static (int swapRemove, int invertRemove, int selfRemove) ComputeGiveAndTake(
        int amount, int swapCap, int invertMax)
    {
        int swapRemove = Math.Max(0, Math.Min(amount, swapCap));
        int invertRemove = Math.Max(0, Math.Min(amount, invertMax));
        int selfRemove = Math.Min(amount, swapRemove + invertRemove);
        return (swapRemove, invertRemove, selfRemove);
    }
}
