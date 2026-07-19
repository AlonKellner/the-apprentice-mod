using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

// (see below for IsInvertible/IsSwappable classification tests)

namespace TheUnderstudy.Tests.Patches;

// Pure-logic tests for the merged Invertible/Swappable tooltip-suffix decision. The actual
// PowerModel.HoverTips mutation and in-combat gating are runtime-only (Harmony) and verified in-game.
public class BasePowerTooltipSuffixTests
{
    [Fact]
    public void Weak_IsBothInvertibleAndSwappable_InThatOrder()
    {
        // The reported bug: Weak showed Invertible but not Swappable. It must get both.
        Assert.Equal(" [gold]Invertible[/gold]. [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new WeakPower(), "Weak."));
    }

    [Fact]
    public void Vulnerable_And_Frail_GetBothSuffixes()
    {
        Assert.Equal(" [gold]Invertible[/gold]. [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new VulnerablePower(), "Vulnerable."));
        Assert.Equal(" [gold]Invertible[/gold]. [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new FrailPower(), "Frail."));
    }

    [Fact]
    public void Strength_And_Vigor_AreBoth()
    {
        // Sign-flip buffs: invertible (InvertVigorSign / Strength sign-flip) AND swappable (stolen
        // from enemies via SwappableBuffs). This is the fix for Vigor not appearing (e.g. on Forte).
        Assert.Equal(" [gold]Invertible[/gold]. [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new StrengthPower(), "Strength."));
        Assert.Equal(" [gold]Invertible[/gold]. [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new VigorPower(), "Vigor."));
    }

    [Fact]
    public void Poison_IsSwappableOnly()
    {
        Assert.Equal(" [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new PoisonPower(), "Poison."));
    }

    [Fact]
    public void Unweak_GetsNoSuffix()
    {
        // Unweak is the mod's own (ICustomModel) power — it self-describes Invertible+Swappable in its
        // PowerLoc, so the base-power patch/helper adds nothing.
        Assert.Equal("", BasePowerTooltipSuffixPatch.MissingSuffix(new UnweakPower(), "Unweak."));
    }

    [Fact]
    public void Idempotent_WhenSuffixAlreadyPresent()
    {
        // If Invertible is already on the tip, only the still-missing Swappable is added.
        Assert.Equal(" [gold]Swappable[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new WeakPower(), "Weak. [gold]Invertible[/gold]."));
        // Both present -> nothing to add.
        Assert.Equal("",
            BasePowerTooltipSuffixPatch.MissingSuffix(new WeakPower(),
                "Weak. [gold]Invertible[/gold]. [gold]Swappable[/gold]."));
    }

    // Shared classification used identically by BOTH the live power-icon patch and the card-tip helper
    // (UnderstudyCard.WithMarkedTip). Mod powers (ICustomModel, e.g. Shaken) are excluded from both
    // sets — they self-describe in their PowerLoc.
    [Fact]
    public void IsSwappable_CoversBaseSwapRegistryPowers_ExcludesModPowers()
    {
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new WeakPower()));       // SwappableDebuffs
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new FrailPower()));      // SwappableDebuffs
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new StrengthPower()));   // SwappableBuffs
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new VigorPower()));      // SwappableBuffs
        Assert.False(BasePowerTooltipSuffixPatch.IsSwappable(new ShakenPower()));    // mod power
    }

    [Fact]
    public void IsInvertible_DerivesFromCanonicalPredicate_ExcludesModPowers()
    {
        Assert.True(BasePowerTooltipSuffixPatch.IsInvertible(new WeakPower()));
        Assert.True(BasePowerTooltipSuffixPatch.IsInvertible(new StrengthPower()));
        Assert.True(BasePowerTooltipSuffixPatch.IsInvertible(new VigorPower()));     // sign-flip invertible
        Assert.False(BasePowerTooltipSuffixPatch.IsInvertible(new PoisonPower()));   // base, but not invertible
        Assert.False(BasePowerTooltipSuffixPatch.IsInvertible(new ShakenPower()));   // mod power (self-describes)
    }
}
