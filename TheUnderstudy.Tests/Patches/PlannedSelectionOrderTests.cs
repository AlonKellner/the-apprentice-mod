using System.Collections.Generic;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Patches;

// The single-player fix for "Planned play order didn't match the badges": appliers pass their selection
// result through PlannedSelectionState.InClickOrder, whose pure core ReorderByClickOrder reorders the
// selection to match the captured click order. These lock that pure reorder (the reactive publish/consume
// wiring itself is in-game only — it needs a live selection screen + Player).
public class PlannedSelectionOrderTests
{
    private static List<string> Reorder(IReadOnlyList<string> selected, IReadOnlyList<string> clickOrder) =>
        PlannedSelectionState.ReorderByClickOrder(selected, clickOrder);

    [Fact]
    public void ClickOrder_ReordersSelectionToMatch()
    {
        // The exact reported case: badges #1 DaCapo, #2 SonicBoom, #3 Exterminate, but the selection
        // result came back in a different (pile/HashSet) order.
        var selected = new[] { "Exterminate", "DaCapo", "SonicBoom" };
        var clickOrder = new[] { "DaCapo", "SonicBoom", "Exterminate" };
        Assert.Equal(new[] { "DaCapo", "SonicBoom", "Exterminate" }, Reorder(selected, clickOrder));
    }

    [Fact]
    public void AlreadyInOrder_Unchanged() =>
        Assert.Equal(new[] { "A", "B", "C" }, Reorder(new[] { "A", "B", "C" }, new[] { "A", "B", "C" }));

    [Fact]
    public void ClickOrderEntriesNotSelected_AreIgnored() =>
        Assert.Equal(new[] { "B", "A" }, Reorder(new[] { "A", "B" }, new[] { "C", "B", "A" }));

    [Fact]
    public void SelectedNotInClickOrder_AppendedInOriginalOrder() =>
        // C leads (click order), then A and B (not clicked) keep their original relative order.
        Assert.Equal(new[] { "C", "A", "B" }, Reorder(new[] { "A", "B", "C" }, new[] { "C" }));

    [Fact]
    public void EmptyClickOrder_ReturnsSelectionUnchanged() =>
        Assert.Equal(new[] { "A", "B" }, Reorder(new[] { "A", "B" }, System.Array.Empty<string>()));
}
