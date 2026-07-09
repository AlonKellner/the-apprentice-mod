using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
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
        // Buildup is Skill + Stable-tagged — ineligible despite being the right CardType.
        var stable = new Buildup();
        var strike = new UnderstudyStrike();
        var (playThis, dontPlayThis, remaining) = SecondLessonPower.SelectFirstTwoEligible(new List<CardModel> { stable, strike });
        Assert.Same(strike, playThis);
        Assert.Null(dontPlayThis);
        Assert.Empty(remaining);
    }
}
