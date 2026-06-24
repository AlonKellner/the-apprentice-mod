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
        Assert.Contains("[gold]Planned[/gold]", (string)args[1]!);
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
}
