using BaseLib.Abstracts;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Patches;

// StableEnforcer freezes what it can snapshot (modifiers + local keywords + replay count) and reverts
// drift. Two mid-combat modifications can't be reverted that way and have to be blocked at the game's own
// gate instead: an Affliction (whose application is one-way — ClearAffliction is a separate effect) and an
// in-combat Upgrade (irreversible: UpgradeInternal rewrites dynamic vars, cost and description).
//
// The Harmony patches themselves aren't unit-testable, but their decisions are pure predicates, so those
// are what the patches delegate to and what is pinned here — the same split TunedPreview uses for
// ShouldApplyOutOfRun. Workshop is the printed-Stable fixture; UnderstudyStrike the plain one.
public class StableProtectionPatchTests
{
    // Afflictions — base-game enemy powers (Hex, Tangled, Chains of Binding, Vital Spark, Ringing,
    // Galvanic) all afflict the player's cards mid-combat, and none of them consult Stable.

    [Fact]
    public void ShouldBlockAffliction_StableCard_ReturnsTrue() =>
        Assert.True(StableAfflictionPatch.ShouldBlock(new Workshop()));

    [Fact]
    public void ShouldBlockAffliction_PlainCard_ReturnsFalse() =>
        Assert.False(StableAfflictionPatch.ShouldBlock(new UnderstudyStrike()));

    // Stable granted at runtime (Final Draft) must protect exactly as much as the printed keyword.
    [Fact]
    public void ShouldBlockAffliction_RuntimeStableCard_ReturnsTrue()
    {
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.True(StableAfflictionPatch.ShouldBlock(card));
    }

    // Upgrades — blocked only inside combat (Armaments, Quasar, Storm of Steel, Compact, Primal Force,
    // Drain, Knife Trap...). Upgrading at a rest site or from an event is not a mid-combat modification and
    // must keep working, or a Stable card could never be upgraded at all.

    [Fact]
    public void ShouldBlockUpgrade_StableCardInCombat_ReturnsTrue() =>
        Assert.True(StableUpgradePatch.ShouldBlock(new Workshop(), inCombatPile: true));

    [Fact]
    public void ShouldBlockUpgrade_StableCardOutsideCombat_ReturnsFalse() =>
        Assert.False(StableUpgradePatch.ShouldBlock(new Workshop(), inCombatPile: false));

    [Fact]
    public void ShouldBlockUpgrade_PlainCardInCombat_ReturnsFalse() =>
        Assert.False(StableUpgradePatch.ShouldBlock(new UnderstudyStrike(), inCombatPile: true));

    [Fact]
    public void ShouldBlockUpgrade_RuntimeStableCardInCombat_ReturnsTrue()
    {
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.True(StableUpgradePatch.ShouldBlock(card, inCombatPile: true));
    }

    // The one-arg overload the patch actually calls derives the pile from the card. A bare card has no
    // pile, which is the "not in combat" case — so it must not block.
    [Fact]
    public void ShouldBlockUpgrade_StableCardWithNoPile_ReturnsFalse() =>
        Assert.False(StableUpgradePatch.ShouldBlock(new Workshop()));
}
