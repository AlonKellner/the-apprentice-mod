using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

// StableEnforcer is the engine-independent core of the Stable "freeze" mechanism: it snapshots a card's
// BaseLib modifiers (with their internal state) plus its local keywords, and reconciles the live card back
// to that snapshot. Everything here runs on bare-constructed cards (no ModelDb / CombatState / Godot),
// which is exactly why the reconciliation logic lives here and not inline in UnderstudyCard's hooks.
public class StableEnforcerTests
{
    private static PlannedModifier PlannedWith(CardModel card, params int[] slots)
    {
        var mod = new PlannedModifier();
        foreach (var s in slots) mod.SequenceIndices.Add(s);
        CardModifier.DirectModifiers(card).Add(mod);
        return mod;
    }

    // The unit-level reproduction of the reported bug: RemoveSlot empties SequenceIndices in place, and a
    // reference snapshot would "restore" the same emptied instance. Deep restore must refill the slots.
    [Fact]
    public void Restore_AfterInPlaceSlotMutation_RestoresSlots()
    {
        var card = new UnderstudyStrike();
        var mod = PlannedWith(card, 3, 7);
        var snap = StableEnforcer.Capture(card);

        mod.SequenceIndices.Clear();

        Assert.True(StableEnforcer.Restore(card, snap));
        Assert.True(card.TryGetModifier<PlannedModifier>(out var m));
        Assert.Equal(new[] { 3, 7 }, m!.SequenceIndices);
    }

    [Fact]
    public void Restore_AfterModifierDetached_ReattachesSameInstanceWithSlots()
    {
        var card = new UnderstudyStrike();
        var mod = PlannedWith(card, 2);
        var snap = StableEnforcer.Capture(card);

        CardModifier.DirectModifiers(card).Remove(mod);

        Assert.True(StableEnforcer.Restore(card, snap));
        Assert.True(card.TryGetModifier<PlannedModifier>(out var m));
        // Must re-add the SAME instance, never construct a new one: `new`-ing a model in a live combat
        // throws DuplicateModelException (ModelDb is initialized). This assertion encodes that constraint.
        Assert.Same(mod, m);
        Assert.Equal(new[] { 2 }, m!.SequenceIndices);
    }

    [Fact]
    public void Restore_StripsForeignModifier_NotInSnapshot()
    {
        var card = new UnderstudyStrike();
        PlannedWith(card, 0);
        var snap = StableEnforcer.Capture(card);

        CardModifier.DirectModifiers(card).Add(new TunedModifier());

        Assert.True(StableEnforcer.Restore(card, snap));
        Assert.False(card.TryGetModifier<TunedModifier>(out _));
        Assert.True(card.TryGetModifier<PlannedModifier>(out _));
    }

    // Keyword *application* (AddKeyword/RemoveKeyword) asserts the card is mutable, which only holds in a
    // live combat — so it's verified in-game. The reconciliation *decision* is pure and tested here.
    [Fact]
    public void KeywordDiff_ForeignKeyword_IsRemoved()
    {
        var frozen = new HashSet<CardKeyword>();
        var current = new HashSet<CardKeyword> { CardKeyword.Ethereal };
        var (toAdd, toRemove) = StableEnforcer.KeywordDiff(frozen, current);
        Assert.Empty(toAdd);
        Assert.Equal(new[] { CardKeyword.Ethereal }, toRemove);
    }

    [Fact]
    public void KeywordDiff_MissingFrozenKeyword_IsReAdded()
    {
        var frozen = new HashSet<CardKeyword> { CardKeyword.Retain };
        var current = new HashSet<CardKeyword>();
        var (toAdd, toRemove) = StableEnforcer.KeywordDiff(frozen, current);
        Assert.Equal(new[] { CardKeyword.Retain }, toAdd);
        Assert.Empty(toRemove);
    }

    [Fact]
    public void KeywordDiff_Equal_ReturnsNothing()
    {
        var frozen = new HashSet<CardKeyword> { CardKeyword.Retain };
        var current = new HashSet<CardKeyword> { CardKeyword.Retain };
        var (toAdd, toRemove) = StableEnforcer.KeywordDiff(frozen, current);
        Assert.Empty(toAdd);
        Assert.Empty(toRemove);
    }

    [Fact]
    public void Restore_NoChanges_ReturnsFalse_AndLeavesCardIntact()
    {
        var card = new UnderstudyStrike();
        PlannedWith(card, 1);
        var snap = StableEnforcer.Capture(card);

        Assert.False(StableEnforcer.Restore(card, snap));
        Assert.True(card.TryGetModifier<PlannedModifier>(out var m));
        Assert.Equal(new[] { 1 }, m!.SequenceIndices);
    }

    // BaseReplayCount is plain CardModel state — not a modifier and not a keyword — so the modifier/keyword
    // snapshot above cannot see it. Master Form (and base-game Hidden Gem / Sword Sage) write it directly
    // mid-combat, which was a hole straight through the freeze. Its setter calls AssertMutable(), so the
    // tests below drive the backing field, the same trick OrderModifierTests uses for the private
    // Affliction setter. Restore's write-back needs a mutable card and is verified in-game.
    private static void ForceReplayCount(CardModel card, int value) =>
        typeof(CardModel).GetField("_baseReplayCount", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(card, value);

    [Fact]
    public void Capture_RecordsBaseReplayCount()
    {
        var card = new UnderstudyStrike();
        ForceReplayCount(card, 2);
        Assert.Equal(2, StableEnforcer.Capture(card).BaseReplayCount);
    }

    // The reported bug: Master Form grants Replay 1 to a Stable card and nothing notices.
    [Fact]
    public void Matches_AfterReplayCountGranted_ReturnsFalse()
    {
        var card = new UnderstudyStrike();
        var snap = StableEnforcer.Capture(card);

        ForceReplayCount(card, 1);

        Assert.False(StableEnforcer.Matches(card, snap));
    }

    // A card that already printed/earned its Replay before becoming Stable keeps it — the snapshot freezes
    // whatever was there, it doesn't force zero.
    [Fact]
    public void Matches_ReplayCountUnchanged_ReturnsTrue()
    {
        var card = new UnderstudyStrike();
        ForceReplayCount(card, 3);
        var snap = StableEnforcer.Capture(card);

        Assert.True(StableEnforcer.Matches(card, snap));
    }

    // The reentrancy guard the Harmony ApplyInternal patch relies on: while enforcing, Restore is inert
    // (re-adds it performs must not recursively trigger another enforcement pass).
    [Fact]
    public void Restore_WhileEnforcing_IsNoOp()
    {
        var card = new UnderstudyStrike();
        var mod = PlannedWith(card, 5);
        var snap = StableEnforcer.Capture(card);
        mod.SequenceIndices.Clear();

        StableEnforcer.Enforcing = true;
        try
        {
            Assert.False(StableEnforcer.Restore(card, snap));
        }
        finally
        {
            StableEnforcer.Enforcing = false;
        }

        // Untouched because the guard short-circuited.
        Assert.True(card.TryGetModifier<PlannedModifier>(out var m));
        Assert.Empty(m!.SequenceIndices);
    }
}
