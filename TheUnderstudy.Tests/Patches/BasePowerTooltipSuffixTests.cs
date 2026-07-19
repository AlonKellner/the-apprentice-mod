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
    public void Strength_IsInvertibleOnly()
    {
        Assert.Equal(" [gold]Invertible[/gold].",
            BasePowerTooltipSuffixPatch.MissingSuffix(new StrengthPower(), "Strength."));
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
        // Unweak is the mod's own power (neither an invertible nor swappable base debuff).
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
    // (UnderstudyCard.WithDebuffTip). Weak/Vulnerable/Frail are swappable base debuffs; the mod's own
    // invertible power Shaken is NOT swappable (and not in the base invertible set either — it carries
    // its "Invertible" wording in its own PowerLoc, so MissingSuffix stays a no-op for it).
    [Fact]
    public void IsSwappable_CoversBaseDebuffs_ExcludesShaken()
    {
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new WeakPower()));
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new VulnerablePower()));
        Assert.True(BasePowerTooltipSuffixPatch.IsSwappable(new FrailPower()));
        Assert.False(BasePowerTooltipSuffixPatch.IsSwappable(new StrengthPower()));
        Assert.False(BasePowerTooltipSuffixPatch.IsSwappable(new ShakenPower()));
    }

    [Fact]
    public void IsInvertible_CoversBaseInvertiblePowers_ExcludesShaken()
    {
        Assert.True(BasePowerTooltipSuffixPatch.IsInvertible(new WeakPower()));
        Assert.True(BasePowerTooltipSuffixPatch.IsInvertible(new StrengthPower()));
        Assert.False(BasePowerTooltipSuffixPatch.IsInvertible(new PoisonPower()));
        Assert.False(BasePowerTooltipSuffixPatch.IsInvertible(new ShakenPower()));
    }
}
