using System.Linq;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Pure ordering logic for the pre-Planned combat-start assignment. The live attach (sequencer slots,
// modifier wiring) needs a ModelDb/combat and is verified in-game; the sequencer itself is covered by
// PlannedSlotSequencerTests.
public class PrePlannedSetupTests
{
    [Fact]
    public void OrderByDeckRank_SortsByRank_StableForTies()
    {
        // (label, deckRank) — two cards share rank 0 (identical duplicate copies).
        var items = new[] { ("a", 2), ("b", 0), ("c", 0), ("d", 1) };
        var ordered = PrePlannedSetup.OrderByDeckRank(items, x => x.Item2).Select(x => x.Item1).ToArray();

        // rank 0 first (b before c — stable), then rank 1 (d), then rank 2 (a).
        Assert.Equal(new[] { "b", "c", "d", "a" }, ordered);
    }

    [Fact]
    public void OrderByDeckRank_AssigningSequentialSlots_IsUniqueAndDeckOrdered()
    {
        var items = new[] { ("x", 5), ("y", 1), ("z", 3) };
        var slots = PrePlannedSetup.OrderByDeckRank(items, x => x.Item2)
            .Select((item, slot) => (item.Item1, slot))
            .ToArray();

        // Deck order y(1) -> z(3) -> x(5) receives slots 0,1,2 — unique, non-overlapping, deck-ordered.
        Assert.Equal(new[] { ("y", 0), ("z", 1), ("x", 2) }, slots);
    }
}
