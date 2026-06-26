using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Modifiers;

// Tests requiring CardModifier.AddModifier<T>() (which queries ModelDb) cannot run
// without full game initialization and are omitted. TryModifyKeywordsInCombat behavior
// is verified in-game.
public class SpentModifierTests
{
    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheApprentice:Spent", SpentModifier.ModifierId);
    }
}
