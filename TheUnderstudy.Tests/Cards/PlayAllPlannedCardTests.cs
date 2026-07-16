using System.IO;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// PlayAllPlannedCard is the abstract base for the "Play all Planned" resolvers (Curtain Call/DaCapo/Remix).
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

    // Regression guard for the Workshop infinite-loop bug: any CARD that resolves the Planned queue
    // (reads the sorted queue AND auto-plays cards from it) MUST extend PlayAllPlannedCard so it gets
    // the once-per-turn guard. Workshop was a queue resolver on `: UnderstudyCard` with no guard, so a
    // Planned+Stable Workshop replayed the growing queue and recursed infinitely. Only top-level card
    // files are scanned: Powers (e.g. VenuePower in Cards/Powers/) also resolve the queue but do so
    // from a once-per-turn turn-boundary hook — they can't be Planned/replayed like a card, so they
    // aren't a recursion vector and need no card guard. Source-scan, no ModelDb needed.
    [Fact]
    public void EveryQueueResolvingCard_ExtendsPlayAllPlannedCard()
    {
        var cardsDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "TheUnderstudyCode", "Cards"));
        foreach (var file in Directory.GetFiles(cardsDir, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var text = File.ReadAllText(file);
            if (!text.Contains("PlannedModifier.GetSorted") || !text.Contains("CardCmd.AutoPlay")) continue;
            Assert.True(
                text.Contains(": PlayAllPlannedCard"),
                $"{Path.GetFileName(file)} resolves the Planned queue but does not extend " +
                "PlayAllPlannedCard — it lacks the once-per-turn guard and can recurse infinitely " +
                "when Planned+Stable.");
        }
    }
}
