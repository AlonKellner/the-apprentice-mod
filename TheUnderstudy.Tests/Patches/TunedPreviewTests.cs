using System.Reflection;
using BaseLib.Abstracts;
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

    private static void SetTunedCreated(int count) =>
        typeof(TunedModifier).GetField("_tunedCreated", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, count);

    [Fact]
    public void TunedParts_CardCarryingModifier_ReturnsStacksAndTotal()
    {
        // Melody is a non-pre-Tuned attack; the modifier drives the result regardless of pre-Tuned.
        var card = new Melody();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        SetStacks(mod, 2);
        SetTunedCreated(3); // total = Stacks * _tunedCreated = 6

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
        var (dynamicBasePart, total) = TunedPreview.TunedParts(new Melody());
        Assert.Equal(0, dynamicBasePart);
        Assert.Equal(0, total);
    }
}
