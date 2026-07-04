using System.Collections;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

public class TheSecondLessonTests
{
    [Fact]
    public void TheSecondLesson_HasExpectedShape()
    {
        var card = new TheSecondLesson();
        Assert.Equal("TheUnderstudy:TheSecondLesson", TheSecondLesson.CardId);
        Assert.Equal(CardType.Power, card.Type);
        Assert.Equal(CardRarity.Rare, card.Rarity);
        Assert.Equal(TargetType.None, card.TargetType);
    }

    [Fact]
    public void TheSecondLesson_BaseCard_DoesNotHaveInnate()
    {
        var card = new TheSecondLesson();
        Assert.DoesNotContain(CardKeyword.Innate, card.Keywords);
    }

    [Fact]
    public void TheSecondLesson_BaseCard_DoesNotHaveRetain()
    {
        var card = new TheSecondLesson();
        Assert.DoesNotContain(CardKeyword.Retain, card.Keywords);
    }

    [Fact]
    public void TheSecondLesson_QueuesRetainKeywordToAddOnUpgrade()
    {
        var card = new TheSecondLesson();
        var field = typeof(ConstructedCardModel).GetField("UpgradeKeywords", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var upgradeKeywords = (IEnumerable)field!.GetValue(card)!;

        bool found = false;
        foreach (var entry in upgradeKeywords)
        {
            var entryType = entry.GetType();
            var keyword = (CardKeyword)entryType.GetField("Item1")!.GetValue(entry)!;
            int upgradeType = Convert.ToInt32(entryType.GetField("Item2")!.GetValue(entry));
            if (keyword == CardKeyword.Retain && upgradeType == 1 /* ConstructedCardModel.UpgradeType.Add */)
                found = true;
        }

        Assert.True(found, "Expected TheSecondLesson to queue CardKeyword.Retain with UpgradeType.Add.");
    }

    [Fact]
    public void TheSecondLesson_DoesNotQueueInnateKeywordOnUpgrade()
    {
        var card = new TheSecondLesson();
        var field = typeof(ConstructedCardModel).GetField("UpgradeKeywords", BindingFlags.NonPublic | BindingFlags.Instance);
        var upgradeKeywords = (IEnumerable)field!.GetValue(card)!;
        foreach (var entry in upgradeKeywords)
        {
            var entryType = entry.GetType();
            var keyword = (CardKeyword)entryType.GetField("Item1")!.GetValue(entry)!;
            Assert.NotEqual(CardKeyword.Innate, keyword);
        }
    }
}
