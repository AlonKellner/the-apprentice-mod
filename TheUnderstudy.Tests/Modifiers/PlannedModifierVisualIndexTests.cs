using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class PlannedModifierVisualIndexTests
{
    [Fact]
    public void AssignVisualIndices_EmptyList_DoesNotThrow()
    {
        PlannedModifier.AssignVisualIndices(new List<(CardModel, PlannedModifier, int)>());
    }

    [Fact]
    public void AssignVisualIndices_SingleSlot_GetsVisualIndexOne()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(7);
        var sorted = new List<(CardModel, PlannedModifier, int)> { (card, mod, 7) };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod.VisualBySeq[7]);
    }

    [Fact]
    public void AssignVisualIndices_NormalizesGappedMechanicalIndices()
    {
        var mod4 = new PlannedModifier(); mod4.SequenceIndices.Add(4);
        var mod5 = new PlannedModifier(); mod5.SequenceIndices.Add(5);
        var mod9 = new PlannedModifier(); mod9.SequenceIndices.Add(9);
        var sorted = new List<(CardModel, PlannedModifier, int)>
        {
            (new UnderstudyStrike(), mod4, 4),
            (new UnderstudyStrike(), mod5, 5),
            (new UnderstudyStrike(), mod9, 9),
        };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod4.VisualBySeq[4]);
        Assert.Equal(2, mod5.VisualBySeq[5]);
        Assert.Equal(3, mod9.VisualBySeq[9]);
    }

    [Fact]
    public void AssignVisualIndices_HandlesDuplicateMechanicalIndices()
    {
        // Each modifier has its own VisualBySeq dictionary, so two mods with the same
        // seqIdx each get their own entry, keyed by position in the sorted list.
        var mod0a = new PlannedModifier(); mod0a.SequenceIndices.Add(0);
        var mod0b = new PlannedModifier(); mod0b.SequenceIndices.Add(0);
        var mod1  = new PlannedModifier(); mod1.SequenceIndices.Add(1);
        var sorted = new List<(CardModel, PlannedModifier, int)>
        {
            (new UnderstudyStrike(), mod0a, 0),
            (new UnderstudyStrike(), mod0b, 0),
            (new UnderstudyStrike(), mod1,  1),
        };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod0a.VisualBySeq[0]);
        Assert.Equal(2, mod0b.VisualBySeq[0]);
        Assert.Equal(3, mod1.VisualBySeq[1]);
    }

    [Fact]
    public void AssignVisualIndices_MultiSlotSameCard_GetsConsecutiveIndices()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(3);
        mod.SequenceIndices.Add(7);
        var sorted = new List<(CardModel, PlannedModifier, int)>
        {
            (card, mod, 3),
            (card, mod, 7),
        };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod.VisualBySeq[3]);
        Assert.Equal(2, mod.VisualBySeq[7]);
    }

    [Fact]
    public void RefreshVisualIndices_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "RefreshVisualIndices", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }
}
