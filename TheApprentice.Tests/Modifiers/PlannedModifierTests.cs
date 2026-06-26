using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// Tests requiring CardModifier.AddModifier<T>() (which queries ModelDb) cannot run
// without full game initialization and are omitted. The behaviors they cover
// (PlannedModifier.Apply sequence index logic, TryModifyKeywordsInCombat) are
// verified in-game.
public class PlannedModifierTests
{
    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheApprentice:Planned", PlannedModifier.ModifierId);
    }

    [Fact]
    public void SequenceIndex_DefaultsToZero()
    {
        var mod = new PlannedModifier();
        Assert.Equal(0, mod.SequenceIndex);
    }

    [Fact]
    public void SequenceIndex_CanBeSetExternally()
    {
        var mod = new PlannedModifier { SequenceIndex = 2 };
        Assert.Equal(2, mod.SequenceIndex);
    }

    [Fact]
    public void ModifyDescriptionPost_UsesGoldFormatting()
    {
        var mod = new PlannedModifier { SequenceIndex = 0 };
        var args = new object?[] { null, "base" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("[gold]Planned #", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsOneBased_Index()
    {
        var mod = new PlannedModifier { SequenceIndex = 2 };
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#3", (string)args[1]!);
    }

    [Fact]
    public void TryModifyKeywordsInCombat_PlannedField_Exists()
    {
        var field = typeof(TheApprentice.TheApprenticeCode.Cards.ApprenticeKeywords).GetField("Planned");
        Assert.NotNull(field);
    }

    [Fact]
    public void CanApplyTo_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "CanApplyTo", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void CanApplyTo_FreshCard_ReturnsTrue()
    {
        Assert.True(PlannedModifier.CanApplyTo(new ApprenticeStrike()));
    }

    [Fact]
    public void GetSorted_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "GetSorted", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void GetSorted_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(PlannedModifier.GetSorted(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void CountIn_EmptyInput_ReturnsZero()
    {
        Assert.Equal(0, PlannedModifier.CountIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void AnyIn_EmptyInput_ReturnsFalse()
    {
        Assert.False(PlannedModifier.AnyIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void VisualIndex_DefaultsToZero()
    {
        Assert.Equal(0, new PlannedModifier().VisualIndex);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsVisualIndexWhenSet()
    {
        var mod = new PlannedModifier { SequenceIndex = 5, VisualIndex = 2 };
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#2", (string)args[1]!);
    }

    [Fact]
    public void Remove_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "Remove", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Remove_ClearsModifierFromCard()
    {
        var card = new ApprenticeStrike();
        var mod = new PlannedModifier { SequenceIndex = 0 };
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());

        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void Remove_FiresChangedEvent()
    {
        var card = new ApprenticeStrike();
        var mod = new PlannedModifier { SequenceIndex = 0 };
        CardModifier.DirectModifiers(card).Add(mod);

        bool fired = false;
        PlannedModifier.Changed += () => fired = true;
        try
        {
            PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());
            Assert.True(fired);
        }
        finally
        {
            PlannedModifier.Changed -= () => fired = true;
        }
    }
}
