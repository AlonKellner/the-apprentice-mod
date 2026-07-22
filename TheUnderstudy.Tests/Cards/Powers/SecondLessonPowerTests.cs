using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards.Powers;

public class SecondLessonPowerTests
{
    [Fact]
    public void SelectFirstTwoEligible_NoCards_ReturnsAllNull()
    {
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel>());
        Assert.Null(playThis);
        Assert.Null(dontPlayThis);
        Assert.Empty(remaining);
    }

    [Fact]
    public void SelectFirstTwoEligible_SameCardDrawnTwice_DoesNotGetBothOrders()
    {
        // A card can land in drawnThisTurn more than once (drawn, leaves hand, drawn again before
        // the list is cleared). Taking eligible[0] and eligible[1] by position then handed the same
        // card both "Play this card" and "Don't play this card" at once.
        var drawnTwice = new UnderstudyStrike();
        var other = new UnderstudyDefend();

        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(
            new List<CardModel> { drawnTwice, drawnTwice, other });

        Assert.Same(drawnTwice, playThis);
        Assert.Same(other, dontPlayThis);
        Assert.NotSame(playThis, dontPlayThis);
        Assert.Empty(remaining);
    }

    [Fact]
    public void SelectFirstTwoEligible_DistinctCopiesOfSameCard_BothStayEligible()
    {
        // Reference equality, not card identity: two separate copies of Strike are two real cards
        // and must each be able to carry their own Order.
        var copyOne = new UnderstudyStrike();
        var copyTwo = new UnderstudyStrike();

        var (playThis, dontPlayThis, _) = SecondLessonPower.SelectFirstTwoEligible(
            new List<CardModel> { copyOne, copyTwo });

        Assert.Same(copyOne, playThis);
        Assert.Same(copyTwo, dontPlayThis);
    }

    [Fact]
    public void SelectFirstTwoEligible_SkipsCardsAnotherLessonAlreadyOrdered()
    {
        // A second Lesson selecting after the first has already taken its two picks: the afflicted
        // cards drop out of the pool, so the Orders land on different cards instead of overlapping.
        var takenByFirstLesson = new UnderstudyStrike();
        var alsoTakenByFirstLesson = new UnderstudyDefend();
        GiveAffliction(takenByFirstLesson, new Order());
        GiveAffliction(alsoTakenByFirstLesson, new Order());
        var stillFree = new UnderstudyStrike();

        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(
            new List<CardModel> { takenByFirstLesson, alsoTakenByFirstLesson, stillFree });

        Assert.Same(stillFree, playThis);
        Assert.Null(dontPlayThis);
        Assert.Empty(remaining);
    }

    // Afflicting for real needs a mutable card, and MutableClone() reaches into ModelDb — neither is
    // available in the bare test host, so drive the private setter that CanApplyTo reads.
    private static void GiveAffliction(CardModel card, AfflictionModel affliction) =>
        typeof(CardModel).GetProperty(nameof(CardModel.Affliction))!
            .GetSetMethod(nonPublic: true)!.Invoke(card, new object?[] { affliction });

    [Fact]
    public void SelectFirstTwoEligible_OneEligibleCard_OnlyGetsPlayThis()
    {
        var strike = new UnderstudyStrike();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { strike });
        Assert.Same(strike, playThis);
        Assert.Null(dontPlayThis);
        Assert.Empty(remaining);
    }

    [Fact]
    public void SelectFirstTwoEligible_TwoEligibleCards_AssignsInDrawOrder()
    {
        var strike = new UnderstudyStrike();
        var defend = new UnderstudyDefend();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { strike, defend });
        Assert.Same(strike, playThis);
        Assert.Same(defend, dontPlayThis);
        Assert.Empty(remaining);
    }

    [Fact]
    public void SelectFirstTwoEligible_ThreeEligibleCards_ThirdIsRemaining()
    {
        var strike = new UnderstudyStrike();
        var defend = new UnderstudyDefend();
        var third = new UnderstudyStrike();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { strike, defend, third });
        Assert.Same(strike, playThis);
        Assert.Same(defend, dontPlayThis);
        Assert.Single(remaining);
        Assert.Same(third, remaining[0]);
    }

    [Fact]
    public void SelectFirstTwoEligible_IneligibleCardsAreSkipped()
    {
        // TheFirstLesson is a Power card — not Attack/Skill, so it's ineligible and must be
        // skipped entirely rather than counted toward the first/second slots.
        var ineligible = new TheFirstLesson();
        var strike = new UnderstudyStrike();
        var defend = new UnderstudyDefend();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { ineligible, strike, defend });
        Assert.Same(strike, playThis);
        Assert.Same(defend, dontPlayThis);
        Assert.Empty(remaining);
    }

    [Fact]
    public void SelectFirstTwoEligible_StableCardIsSkipped()
    {
        // Practice is Skill + Stable-tagged — ineligible despite being the right CardType.
        var stable = new Practice();
        var strike = new UnderstudyStrike();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { stable, strike });
        Assert.Same(strike, playThis);
        Assert.Null(dontPlayThis);
        Assert.Empty(remaining);
    }
}
