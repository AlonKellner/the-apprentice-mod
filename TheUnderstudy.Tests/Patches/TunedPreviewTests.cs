using System.Collections.Generic;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Patches;

// Bare-instantiation tests for the dynamic-base decomposition (TunedPreview.TunedParts). The actual
// EnchantedValue/PreviewValue mutation and the resulting card color are runtime-only (Harmony + turn
// timing) and are verified in-game, per the repo's no-combat-harness note.
public class TunedPreviewTests
{
    private static void SetStacks(TunedModifier mod, int stacks) =>
        typeof(TunedModifier).GetProperty(nameof(TunedModifier.Stacks))!.SetValue(mod, stacks);

    // See TunedModifierTests: the live card count needs a combat the bare test host can't build.
    private sealed class TunedWithCardCount : TunedModifier
    {
        private readonly int _count;
        public TunedWithCardCount(int count) => _count = count;
        protected override int TunedCardCount() => _count;
    }

    [Fact]
    public void TunedParts_CardCarryingModifier_ReturnsStacksAndTotal()
    {
        // Callback is a non-pre-Tuned attack; the modifier drives the result regardless of pre-Tuned.
        var card = new Callback();
        var mod = new TunedWithCardCount(3);
        CardModifier.AddModifier(card, mod);
        SetStacks(mod, 2); // total = Stacks * Tuned card count = 6

        var (dynamicBasePart, total) = TunedPreview.TunedParts(card);
        Assert.Equal(2, dynamicBasePart); // the card's own Tuned amount (Stacks)
        Assert.Equal(6, total);           // full Tuned damage bonus
    }

    [Fact]
    public void TunedParts_BarePreTunedCard_ReturnsOneOne()
    {
        // Out of combat a pre-Tuned card carries no modifier yet but starts each combat Tuned 1.
        var (dynamicBasePart, total) = TunedPreview.TunedParts(new Practice());
        Assert.Equal(1, dynamicBasePart);
        Assert.Equal(1, total);
    }

    [Fact]
    public void TunedParts_BareNonPreTunedCard_ReturnsZeroZero()
    {
        var (dynamicBasePart, total) = TunedPreview.TunedParts(new Callback());
        Assert.Equal(0, dynamicBasePart);
        Assert.Equal(0, total);
    }

    // ── Out-of-run preview (the Compendium) ─────────────────────────────────────────────────────────
    // CardModel.UpdateDynamicVarPreview opens with `if (RunState == null && CombatState == null) return;`
    // so on every Compendium surface the game runs NO dynamic-var preview at all, and the
    // DamageVar/BlockVar postfixes above never fire. A pre-Tuned card therefore showed its printed base
    // there while showing base+1 in a card reward (which is inside a run). These cover the seam the
    // Harmony postfix calls to close that gap; the postfix attribute itself is runtime-only.

    private static DamageVar Damage(int baseValue) => new(baseValue, ValueProp.Move);

    [Fact]
    public void ShouldApplyOutOfRun_BareCard_IsTrue() =>
        // Both RunState and CombatState are `_owner?.…` on CardModel, so they are safe to read on a
        // canonical card — unlike Owner, which asserts mutability and throws.
        Assert.True(TunedPreview.ShouldApplyOutOfRun(new Practice()));

    [Fact]
    public void ApplyOutOfRun_PreTunedCard_PreviewsTheTunedBonus()
    {
        var damage = Damage(0);

        TunedPreview.ApplyOutOfRun(new DynamicVar[] { damage }, new Practice());

        Assert.Equal(1, (int)damage.PreviewValue);
        // Equal to PreviewValue, so ToHighlightedString colours it neutral: the +1 reads as part of the
        // card (it always has it), not as a buff — matching the card-reward screen.
        Assert.Equal(1, (int)damage.EnchantedValue);
    }

    // The trap this seam exists to avoid. NCard.UpdateVisuals calls DynamicVars.ClearPreview() before
    // the preview, so that path self-cleans — but NCardLibrary's search-text filter calls
    // UpdateDynamicVarPreview with no reset, once per card per keystroke, and TunedPreview.Add does
    // `PreviewValue += total`. Without the reset inside ApplyOutOfRun this climbs 1, 2, 3...
    [Fact]
    public void ApplyOutOfRun_RepeatedCalls_DoNotAccumulate()
    {
        var damage = Damage(0);
        var card = new Practice();

        TunedPreview.ApplyOutOfRun(new DynamicVar[] { damage }, card);
        TunedPreview.ApplyOutOfRun(new DynamicVar[] { damage }, card);
        TunedPreview.ApplyOutOfRun(new DynamicVar[] { damage }, card);

        Assert.Equal(1, (int)damage.PreviewValue);
    }

    // Regression for the One-Up bug: another preview pass could leave the Tuned bonus already in
    // PreviewValue before Add ran (One-Up previewed 2 in the draw/deck view but 1 in hand). Add must
    // assign PreviewValue absolutely (baseline + total), not `+= total`, so a pre-seeded value can't
    // double it.
    [Fact]
    public void Add_PreSeededPreviewValue_DoesNotDoubleCount()
    {
        var damage = Damage(0);
        damage.PreviewValue = 1; // pretend an earlier pass already showed the +1 Tuned bonus

        TunedPreview.Add(damage, new Practice(), runGlobalHooks: false);

        Assert.Equal(1, (int)damage.PreviewValue); // base 0 + Tuned 1, NOT 2
        Assert.Equal(1, (int)damage.EnchantedValue);
    }

    [Fact]
    public void ApplyOutOfRun_NonPreTunedCard_LeavesDamageAlone()
    {
        var damage = Damage(9);

        TunedPreview.ApplyOutOfRun(new DynamicVar[] { damage }, new Callback());

        Assert.Equal(9, (int)damage.PreviewValue);
        Assert.Equal(9, (int)damage.EnchantedValue);
    }

    [Fact]
    public void ApplyOutOfRun_BlockVar_GetsTheSameTreatment()
    {
        var block = new BlockVar(0, ValueProp.Move);

        TunedPreview.ApplyOutOfRun(new DynamicVar[] { block }, new Practice());

        Assert.Equal(1, (int)block.PreviewValue);
    }

    // Only damage and block carry the Tuned bonus; a card's other vars (Practice's own "Select", card
    // counts, energy...) must not be touched.
    [Fact]
    public void ApplyOutOfRun_OtherVarTypes_AreUntouched()
    {
        var other = new IntVar("Select", 4);

        TunedPreview.ApplyOutOfRun(new DynamicVar[] { other }, new Practice());

        Assert.Equal(4, (int)other.PreviewValue);
        Assert.Equal(4, (int)other.EnchantedValue);
    }

    // ── Pre-Tuned cards whose whole damage IS the Tuned bonus ───────────────────────────────────────
    // These three print 0 and read 1: a pre-Tuned card starts each combat carrying Tuned 1, and Tuned
    // adds Stacks per card with Tuned — counting itself. Printing 1 would make them read 2.
    //
    // The other pre-Tuned cards (Clean Slate 3, Shower Thought 2, Showstopper 27) print
    // real numbers on top of that and are deliberately not in this list.
    public static IEnumerable<object[]> ZeroBasePreTunedCards() => new[]
    {
        new object[] { new Practice() },
        new object[] { new Experience() },
        new object[] { new OneUp() },
    };

    [Theory]
    [MemberData(nameof(ZeroBasePreTunedCards))]
    public void ZeroBasePreTunedCard_PrintsZero(UnderstudyCard card) =>
        Assert.Equal(0, (int)card.DynamicVars.Damage.BaseValue);

    // ...and the two halves agree: printed 0 + the pre-Tuned self-bonus reads as 1 on every out-of-run
    // surface, which is what the card-reward screen has always shown.
    [Theory]
    [MemberData(nameof(ZeroBasePreTunedCards))]
    public void ZeroBasePreTunedCard_PreviewsAsOne(UnderstudyCard card)
    {
        TunedPreview.ApplyOutOfRun(card.DynamicVars.Values, card);

        Assert.Equal(1, (int)card.DynamicVars.Damage.PreviewValue);
    }
}
