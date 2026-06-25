using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

public class PlannedModifierVisualIndexTests
{
    [Fact]
    public void AssignVisualIndices_EmptyList_DoesNotThrow()
    {
        PlannedModifier.AssignVisualIndices(new List<(CardModel, PlannedModifier)>());
    }

    [Fact]
    public void AssignVisualIndices_SingleCard_GetsVisualIndexOne()
    {
        var mod = new PlannedModifier { SequenceIndex = 7 };
        var sorted = new List<(CardModel, PlannedModifier)> { (new ApprenticeStrike(), mod) };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod.VisualIndex);
    }

    [Fact]
    public void AssignVisualIndices_NormalizesGappedMechanicalIndices()
    {
        var mod4 = new PlannedModifier { SequenceIndex = 4 };
        var mod5 = new PlannedModifier { SequenceIndex = 5 };
        var mod9 = new PlannedModifier { SequenceIndex = 9 };
        var sorted = new List<(CardModel, PlannedModifier)>
        {
            (new ApprenticeStrike(), mod4),
            (new ApprenticeStrike(), mod5),
            (new ApprenticeStrike(), mod9)
        };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod4.VisualIndex);
        Assert.Equal(2, mod5.VisualIndex);
        Assert.Equal(3, mod9.VisualIndex);
    }

    [Fact]
    public void AssignVisualIndices_HandlesDuplicateMechanicalIndices()
    {
        var mod0a = new PlannedModifier { SequenceIndex = 0 };
        var mod0b = new PlannedModifier { SequenceIndex = 0 };
        var mod1 = new PlannedModifier { SequenceIndex = 1 };
        var sorted = new List<(CardModel, PlannedModifier)>
        {
            (new ApprenticeStrike(), mod0a),
            (new ApprenticeStrike(), mod0b),
            (new ApprenticeStrike(), mod1)
        };
        PlannedModifier.AssignVisualIndices(sorted);
        Assert.Equal(1, mod0a.VisualIndex);
        Assert.Equal(2, mod0b.VisualIndex);
        Assert.Equal(3, mod1.VisualIndex);
    }

    [Fact]
    public void RefreshVisualIndices_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "RefreshVisualIndices", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }
}
