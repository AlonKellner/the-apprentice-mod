using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// "Best of Both": Swap and Invert resolved SIMULTANEOUSLY. For each of your debuffs, both effects read the
// same starting amount — Invert flips up to {Invert} of it into a buff on you, and Swap pushes it onto every
// enemy — then the debuff is stripped once. So from 1 Weak you become Unweak AND every enemy gains Weak,
// instead of Swap consuming the Weak before Invert could flip it. Swap's TAKE half also runs, stealing each
// enemy's buff — so you keep the good side of your debuffs and grab theirs too. (Swap = Audience, Invert = Self.)
public class BestOfBoth : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BestOfBoth";

    public BestOfBoth() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Swap", 1), new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
        WithTip(UnderstudyKeywords.SwapAndInvert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m); // Swap -> Swap twice
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await ResolveFor(context, cardPlay.Card.Owner.Creature,
            (int)DynamicVars["Swap"].BaseValue, (int)DynamicVars["Invert"].BaseValue);

    // The full Best of Both resolution for one creature — shared with Pass the Mic, which runs the exact same thing
    // for a targeted teammate. Interleaved Swap + Invert on the SAME capture -> remove -> apply pipeline as
    // regular Swap (SceneStealing): capture each debuff's give+invert and each enemy's buff to take from the
    // current state, then remove from both sides, then apply to both sides — so interacting powers (an
    // enemy's Artifact, a Weak/Unweak pair) swap instead of cancelling before they're moved.
    public static Task ResolveFor(
        PlayerChoiceContext context, Creature creature, int swapStacks, int invertMax) =>
        Resolve(context, creature, swapStacks, invertMax);

    // "Swap ALL & Invert ALL" — Standing Ovation. Both halves are uncapped (int.MaxValue), which resolves
    // to: swap every swappable debuff on the player and every swappable buff on the enemies (full stacks),
    // and invert the full stacks of every self-debuff. Deliberately game-breaking; it is an Ancient card.
    public static Task ResolveAllFor(PlayerChoiceContext context, Creature creature) =>
        Resolve(context, creature, int.MaxValue, int.MaxValue);

    // Shared core. Swap moves FULL STACKS, `swapStacks` distinct ones (int.MaxValue = all) — the same
    // per-stack rule as regular Swap — while Invert still flips up to `invertMax` of EVERY invertible
    // debuff, simultaneously, on one capture -> remove -> apply plan. So the swap-give only lands on the
    // swapStacks rightmost giveable debuffs, but every debuff can still be inverted; and the take pulls the
    // swapStacks rightmost buffs off each enemy. Passing int.MaxValue as a give-cap means "the whole stack".
    private static async Task Resolve(
        PlayerChoiceContext context, Creature creature, int swapStacks, int invertMax)
    {
        var enemies = creature.CombatState!.HittableEnemies.ToList();
        var swapGive = SelectSwapGivePairs(creature, swapStacks); // which debuffs get the full swap-give

        var plan = new SceneStealing.SwapPlan();
        foreach (var pair in InvertiblePairs.All)
            pair.CaptureGiveAndInvert(
                plan, creature, enemies, swapGive.Contains(pair) ? int.MaxValue : 0, invertMax);
        SceneStealing.CaptureTake(plan, creature, enemies, swapStacks);
        await SceneStealing.ExecutePlan(context, creature, plan);
    }

    // The `n` rightmost invertible pairs whose debuff is present AND swap-giveable on `self` (Strength/
    // Dexterity are invertible but not giveable, so they never appear here — they stay invert-only). Ranked
    // by the debuff's position in the synced Powers list, exactly like regular Swap's rightmost selection,
    // so the choice is multiplayer-deterministic. n = int.MaxValue selects all of them (Swap ALL).
    private static HashSet<InvertiblePair> SelectSwapGivePairs(Creature self, int n)
    {
        var candidates = new List<InvertiblePair>();
        foreach (var pair in InvertiblePairs.All)
            if (pair.DebuffHoldingOn(self) is { } h && SceneStealing.IsGiveable(h)) candidates.Add(pair);

        var positions = candidates
            .Select(p => SceneStealing.PowerPosition(self, p.DebuffHoldingOn(self)!.Value.Power.Id)).ToList();
        return SceneStealing.SelectRightmostN(positions, n).Select(i => candidates[i]).ToHashSet();
    }
}
