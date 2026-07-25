using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

// Pins the pure core of Planned slot assignment (PlannedModifier.NextSlotFromExisting): the next slot
// is always "highest already handed out + 1", never a reuse. The full per-owner assignment
// (NextSlotFor) reads a live ModelDb/Player graph and is verified in-game — see project memory
// "No combat test harness exists". Slots are derived from the checksummed model graph (not a
// process-global counter) precisely so they stay identical across multiplayer clients.
public class PlannedSlotAssignmentTests
{
    [Fact]
    public void NextSlotFromExisting_NoSlots_StartsAtZero()
    {
        Assert.Equal(0, PlannedModifier.NextSlotFromExisting(new int[0]));
    }

    [Fact]
    public void NextSlotFromExisting_ContiguousSlots_ReturnsMaxPlusOne()
    {
        Assert.Equal(3, PlannedModifier.NextSlotFromExisting(new[] { 0, 1, 2 }));
    }

    [Fact]
    public void NextSlotFromExisting_SparseOrOutOfOrder_ReturnsMaxPlusOne()
    {
        // A mid-combat reload may restore slots up to 5 with gaps; the next must be 6, never a gap-fill
        // that could collide with a restored slot.
        Assert.Equal(6, PlannedModifier.NextSlotFromExisting(new[] { 5, 0, 3 }));
    }

    [Fact]
    public void NextSlotFromExisting_SingleSlot_ReturnsNext()
    {
        Assert.Equal(1, PlannedModifier.NextSlotFromExisting(new[] { 0 }));
    }
}
