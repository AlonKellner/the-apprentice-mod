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

// Tests requiring CardModifier.AddModifier<T>() (which queries ModelDb) cannot run
// without full game initialization and are omitted. The behaviors they cover
// (PlannedModifier.Apply sequence index logic, TryModifyKeywordsInCombat) are
// verified in-game.
public class PlannedModifierTests
{
    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheUnderstudy:Planned", PlannedModifier.ModifierId);
    }

    [Fact]
    public void SequenceIndices_DefaultsToEmpty()
    {
        var mod = new PlannedModifier();
        Assert.Empty(mod.SequenceIndices);
    }

    [Fact]
    public void SequenceIndices_CanAddEntry()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(2);
        Assert.Equal(2, mod.SequenceIndices[0]);
    }

    [Fact]
    public void ModifyDescriptionPost_UsesGoldFormatting()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        var args = new object?[] { null, "base" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("[gold]Planned #", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsOneBased_Index()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(2);
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#3", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsBothSlots_WhenCardIsMultiPlanned()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(3);
        mod.SequenceIndices.Add(7);
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        var desc = (string)args[1]!;
        Assert.Contains("#4", desc);
        Assert.Contains("#8", desc);
    }

    [Fact]
    public void TryModifyKeywordsInCombat_PlannedField_Exists()
    {
        var field = typeof(TheUnderstudy.TheUnderstudyCode.Cards.UnderstudyKeywords).GetField("Planned");
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
    public void CanApplyTo_AttackCard_ReturnsTrue()
    {
        Assert.True(PlannedModifier.CanApplyTo(new UnderstudyStrike()));
    }

    [Fact]
    public void CanApplyTo_SkillCard_ReturnsTrue()
    {
        Assert.True(PlannedModifier.CanApplyTo(new UnderstudyDefend()));
    }

    [Fact]
    public void CanApplyTo_AlreadyPlannedAttack_ReturnsTrue()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);
        Assert.True(PlannedModifier.CanApplyTo(card));
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
    public void GetSorted_CardWithTwoSlots_ReturnsTwoEntries()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);
        var sorted = PlannedModifier.GetSorted(new[] { card });
        Assert.Equal(2, sorted.Count);
        Assert.Equal(0, sorted[0].slotSeqIdx);
        Assert.Equal(3, sorted[1].slotSeqIdx);
    }

    [Fact]
    public void CountIn_EmptyInput_ReturnsZero()
    {
        Assert.Equal(0, PlannedModifier.CountIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void CountIn_CountsDistinctCards_NotSlots()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);
        Assert.Equal(1, PlannedModifier.CountIn(new[] { card }));
    }

    [Fact]
    public void AnyIn_EmptyInput_ReturnsFalse()
    {
        Assert.False(PlannedModifier.AnyIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void VisualBySeq_DefaultsToEmpty()
    {
        Assert.Empty(new PlannedModifier().VisualBySeq);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsVisualIndexWhenSet()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(5);
        mod.VisualBySeq[5] = 2;
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
    public void RemoveSlot_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "RemoveSlot", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Remove_ClearsModifierFromCard()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());

        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void RemoveSlot_RemovesSpecificSlot_KeepsOther()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.RemoveSlot(card, 0, Enumerable.Empty<CardModel>());

        Assert.True(card.TryGetModifier<PlannedModifier>(out var remaining));
        Assert.Equal(new[] { 3 }, remaining!.SequenceIndices);
    }

    [Fact]
    public void RemoveSlot_RemovesModifier_WhenLastSlotGone()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.RemoveSlot(card, 0, Enumerable.Empty<CardModel>());

        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void Remove_FiresChangedEvent()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
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

    [Fact]
    public void RelevantCards_NullPlayer_ReturnsEmpty()
    {
        Assert.Empty(PlannedModifier.RelevantCards(null));
    }
}
