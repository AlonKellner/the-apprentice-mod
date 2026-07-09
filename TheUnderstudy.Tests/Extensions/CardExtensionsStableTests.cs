using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Xunit;

namespace TheUnderstudy.Tests.Extensions;

public class CardExtensionsStableTests
{
    [Fact]
    public void IsStable_BareCard_ReturnsFalse()
    {
        Assert.False(((CardModel)new UnderstudyStrike()).IsStable());
    }

    [Fact]
    public void IsStable_AfterStableModifierApply_ReturnsTrue()
    {
        // Non-generic AddModifier overload — the generic StableModifier.Apply() requires a
        // ModelDb lookup unavailable in the bare test host (see project memory on test constraints).
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.True(((CardModel)card).IsStable());
    }

    [Fact]
    public void IsStable_PrintedStableCard_ReturnsTrue()
    {
        // Buildup is Stable via WithKeyword in its constructor (the static/printed path).
        Assert.True(((CardModel)new Buildup()).IsStable());
    }
}
