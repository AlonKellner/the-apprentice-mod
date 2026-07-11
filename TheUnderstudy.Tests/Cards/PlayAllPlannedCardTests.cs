using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// PlayAllPlannedCard is the abstract base for the "Play all Planned" resolvers (Curtain Call/Encore/Remix).
// Its once-per-turn guard is what both caps the effect and — because it marks BEFORE the queue is played —
// breaks the infinite recursion when such a resolver is itself Planned+Stable. The guard/reset logic is
// engine-independent, so it's unit-tested here on bare instances; the hook wiring and the real recursion
// break are verified in-game.
public class PlayAllPlannedCardTests
{
#pragma warning disable STS001 // test-only stub — no localization entry needed
    private sealed class TestResolver : PlayAllPlannedCard
    {
        public TestResolver() : base(0, CardType.Skill, CardRarity.Common, TargetType.None) { }

        public bool Begin() => BeginPlayAllThisTurn();
        public void ResetTurn() => ResetPlayAllThisTurn();
    }
#pragma warning restore STS001

    [Fact]
    public void BeginPlayAllThisTurn_FirstCall_ReturnsTrue()
    {
        Assert.True(new TestResolver().Begin());
    }

    [Fact]
    public void BeginPlayAllThisTurn_SubsequentCallsSameTurn_ReturnFalse()
    {
        var card = new TestResolver();
        Assert.True(card.Begin());
        Assert.False(card.Begin());
        Assert.False(card.Begin());
    }

    [Fact]
    public void ResetPlayAllThisTurn_ReEnablesTheGuard()
    {
        var card = new TestResolver();
        Assert.True(card.Begin());
        Assert.False(card.Begin());

        card.ResetTurn();

        Assert.True(card.Begin());
    }

    [Fact]
    public void Guard_IsPerInstance_NotGlobal()
    {
        var a = new TestResolver();
        var b = new TestResolver();

        Assert.True(a.Begin());
        // b is unaffected by a being marked — each card gets its own once-per-turn.
        Assert.True(b.Begin());
    }
}
