using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// Tests that instantiate PlannedModifier (extends CardModifier from BaseLib/sts2) are skipped
// on ARM64 because sts2.dll is x86_64-only.
public class PlannedModifierTests
{
    private const string SkipReason = "Requires sts2.dll (x86_64) which cannot load in arm64 test runner";

    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheApprentice:Planned", PlannedModifier.ModifierId);
    }

    [Fact(Skip = SkipReason)]
    public void SequenceIndex_DefaultsToZero()
    {
        var mod = new PlannedModifier();
        Assert.Equal(0, mod.SequenceIndex);
    }

    [Fact(Skip = SkipReason)]
    public void SequenceIndex_CanBeSetExternally()
    {
        var mod = new PlannedModifier { SequenceIndex = 2 };
        Assert.Equal(2, mod.SequenceIndex);
    }

    [Fact(Skip = SkipReason)]
    public void ModifyDescriptionPost_UsesGoldFormatting()
    {
        var mod = new PlannedModifier { SequenceIndex = 0 };
        // Invoke via reflection to avoid a compile-time reference to Creature (sts2.dll).
        var args = new object?[] { null, "base" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("[gold]Planned[/gold]", (string)args[1]!);
    }

    [Fact(Skip = SkipReason)]
    public void ModifyDescriptionPost_ShowsOneBased_Index()
    {
        var mod = new PlannedModifier { SequenceIndex = 2 };
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#3", (string)args[1]!);
    }

    [Fact(Skip = SkipReason)]
    public void TryModifyKeywordsInCombat_PlannedField_Exists()
    {
        var field = typeof(ApprenticeKeywords).GetField("Planned");
        Assert.NotNull(field);
    }
}
