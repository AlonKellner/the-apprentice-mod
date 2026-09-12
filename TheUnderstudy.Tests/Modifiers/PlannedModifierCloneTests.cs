using System.Reflection;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

// Regression for the Music Box (card-copy) slot collision: a copied card must NOT share its Planned queue
// state with the original. BaseLib clones a card's modifiers via a SHALLOW MutableClone and then calls
// AfterClonedOnCard; PlannedModifier.AfterClonedOnCard must deep-copy _sequenceIndices / _visualBySeq so the
// two cards' slots are independent. Before the fix the clone aliased the original's list ("sharesList=True"
// in the invariant), and applying Planned to one desynced both onto the same slots [5,8].
public class PlannedModifierCloneTests
{
    private static readonly FieldInfo SeqField =
        typeof(PlannedModifier).GetField("_sequenceIndices", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo VisualField =
        typeof(PlannedModifier).GetField("_visualBySeq", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public void AfterClonedOnCard_UnaliasesCollectionsFromTheOriginal()
    {
        var original = new PlannedModifier();
        original.SequenceIndices.Add(5);
        original.VisualBySeq[5] = 1;

        // Reproduce the shallow-clone aliasing a card copy produces: the clone's backing fields reference
        // the original's collections.
        var clone = new PlannedModifier();
        SeqField.SetValue(clone, original.SequenceIndices);
        VisualField.SetValue(clone, original.VisualBySeq);
        Assert.Same(original.SequenceIndices, clone.SequenceIndices);

        clone.AfterClonedOnCard(null!); // the hook BaseLib calls right after cloning the modifier onto the copy

        Assert.NotSame(original.SequenceIndices, clone.SequenceIndices);
        Assert.NotSame(original.VisualBySeq, clone.VisualBySeq);
        Assert.Equal(new[] { 5 }, clone.SequenceIndices); // contents preserved on the copy

        // The actual bug: mutating the copy's plan also mutated the original's shared list.
        clone.SequenceIndices.Add(8);
        Assert.Equal(new[] { 5 }, original.SequenceIndices);
        Assert.Equal(new[] { 5, 8 }, clone.SequenceIndices);
    }
}
