using System.Linq;
using TheUnderstudy.TheUnderstudyCode.Map;
using Xunit;

namespace TheUnderstudy.Tests.Map;

// The Book of Endings' study bank. Studies accumulate; each act draws from the bank up to that act's own
// alt-boss capacity, and whatever the act could not absorb carries into the next one — so no Study is
// ever wasted. All of it is integer arithmetic on purpose: the map injection that consumes these numbers
// needs a live ActMap and calls Log.*, neither of which the bare test host can run, so the rules live
// here where they can actually be pinned.
//
// Two independent mechanisms sit on top of this (see BookOfEndings): the rest-site Study button is
// disabled once Bank == cap, which is what normally stops over-studying, and the carry-over below is the
// safety net for the co-op race where two players both Study before the map redraws.
public class AltBossRevealTests
{
    // The counter/reveal table for the normal 2-alt-boss act, straight from the design:
    //   bank 1 -> 1 revealed, settles to 0
    //   bank 2 -> 2 revealed, settles to 0
    //   bank 3 -> 2 revealed (the cap holds), settles to 1 -- carried into the next act
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(2, 2, 0)]
    [InlineData(3, 2, 1)]
    public void CapTwoAct_RevealsUpToTheCap_AndCarriesTheSurplus(
        int studies, int expectedRevealed, int expectedBankAfterSettling)
    {
        const int cap = 2;
        Assert.Equal(expectedRevealed, AltBossReveal.RevealedInAct(studies, consumed: 0, cap));

        int consumedAfter = AltBossReveal.Settle(studies, consumed: 0, cap);
        Assert.Equal(expectedBankAfterSettling, AltBossReveal.Bank(studies, consumedAfter));
    }

    [Theory]
    [InlineData(0, 0, 2, 0)]  // nothing banked, nothing revealed
    [InlineData(1, 0, 2, 1)]  // clamped by the bank
    [InlineData(5, 0, 2, 2)]  // clamped by the cap
    [InlineData(5, 4, 2, 1)]  // consumed eats into the bank first
    [InlineData(3, 0, 0, 0)]  // a one-boss act has no capacity at all
    public void RevealedInAct_ClampsToBothTheBankAndTheCap(
        int studies, int consumed, int cap, int expected) =>
        Assert.Equal(expected, AltBossReveal.RevealedInAct(studies, consumed, cap));

    // Consumed should never exceed Studies, but a negative bank would silently turn into "reveal
    // nothing forever" rather than an obvious failure, so clamp it.
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(3, 1, 2)]
    [InlineData(1, 4, 0)]
    public void Bank_IsNeverNegative(int studies, int consumed, int expected) =>
        Assert.Equal(expected, AltBossReveal.Bank(studies, consumed));

    // The primary gate: Study stays usable only while this act can still absorb another reveal.
    [Theory]
    [InlineData(0, 0, 2, true)]
    [InlineData(1, 0, 2, true)]
    [InlineData(2, 0, 2, false)]  // act is full
    [InlineData(3, 0, 2, false)]  // still full, surplus banked
    [InlineData(2, 2, 2, true)]   // previous act settled; this act is fresh
    [InlineData(0, 0, 1, true)]
    [InlineData(1, 0, 1, false)]
    [InlineData(0, 0, 0, false)]  // a one-boss act never enables Study
    public void CanStudy_IsFalseExactlyWhenTheActIsFull(
        int studies, int consumed, int cap, bool expected) =>
        Assert.Equal(expected, AltBossReveal.CanStudy(studies, consumed, cap));

    [Theory]
    [InlineData(2, 0, 2, 2)]  // act absorbed both
    [InlineData(1, 0, 2, 1)]  // act absorbed the one available
    [InlineData(3, 0, 2, 2)]  // act absorbed only its cap
    [InlineData(1, 0, 0, 0)]  // no capacity: nothing consumed, bank untouched
    public void Settle_ConsumesOnlyWhatTheActAbsorbed(
        int studies, int consumed, int cap, int expectedConsumed) =>
        Assert.Equal(expectedConsumed, AltBossReveal.Settle(studies, consumed, cap));

    // The full carry-over story end to end: a co-op race banks 3 studies in a 2-boss act, the act shows
    // 2, and the leftover study reveals a boss in the following (single-alt-boss) act. Nothing is lost
    // and nothing is double-spent.
    [Fact]
    public void ARunSequence_NeverLosesAStudy()
    {
        int studies = 3, consumed = 0;

        Assert.Equal(2, AltBossReveal.RevealedInAct(studies, consumed, capThisAct: 2));
        consumed = AltBossReveal.Settle(studies, consumed, capOfActBeingLeft: 2);
        Assert.Equal(1, AltBossReveal.Bank(studies, consumed));

        // Next act only has one alternative boss to give; the carried study pays for it exactly.
        Assert.Equal(1, AltBossReveal.RevealedInAct(studies, consumed, capThisAct: 1));
        Assert.False(AltBossReveal.CanStudy(studies, consumed, capThisAct: 1));
        consumed = AltBossReveal.Settle(studies, consumed, capOfActBeingLeft: 1);

        Assert.Equal(0, AltBossReveal.Bank(studies, consumed));
        Assert.Equal(studies, consumed); // every study ended up spent on a revealed boss
    }

    // The cap is party-wide, not per player: two co-op holders with one study each fill a 2-boss act
    // between them.
    [Fact]
    public void PartyStudies_SumsAcrossHolders()
    {
        int studies = AltBossReveal.PartyStudies(new[] { 1, 1 });

        Assert.Equal(2, studies);
        Assert.Equal(2, AltBossReveal.RevealedInAct(studies, consumed: 0, capThisAct: 2));
        Assert.False(AltBossReveal.CanStudy(studies, consumed: 0, capThisAct: 2));
    }

    [Fact]
    public void PartyStudies_OfNoHolders_IsZero() =>
        Assert.Equal(0, AltBossReveal.PartyStudies(Enumerable.Empty<int>()));
}
