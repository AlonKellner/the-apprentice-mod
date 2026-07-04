using System.Collections;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

public class TheFinalLessonTests
{
    [Fact]
    public void TheFinalLesson_HasExpectedShape()
    {
        var card = new TheFinalLesson();
        Assert.Equal("TheUnderstudy:TheFinalLesson", TheFinalLesson.CardId);
        Assert.Equal(CardType.Power, card.Type);
        Assert.Equal(CardRarity.Rare, card.Rarity);
        Assert.Equal(TargetType.None, card.TargetType);
    }

    [Fact]
    public void TheFinalLesson_HasRetainImmediately()
    {
        var card = new TheFinalLesson();
        Assert.Contains(CardKeyword.Retain, card.Keywords);
    }

    [Fact]
    public void TheFinalLesson_BaseCard_DoesNotHaveInnate()
    {
        var card = new TheFinalLesson();
        Assert.DoesNotContain(CardKeyword.Innate, card.Keywords);
    }

    [Fact]
    public void TheFinalLesson_QueuesInnateKeywordToAddOnUpgrade()
    {
        var card = new TheFinalLesson();
        var field = typeof(ConstructedCardModel).GetField("UpgradeKeywords", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var upgradeKeywords = (IEnumerable)field!.GetValue(card)!;

        bool found = false;
        foreach (var entry in upgradeKeywords)
        {
            var entryType = entry.GetType();
            var keyword = (CardKeyword)entryType.GetField("Item1")!.GetValue(entry)!;
            int upgradeType = Convert.ToInt32(entryType.GetField("Item2")!.GetValue(entry));
            if (keyword == CardKeyword.Innate && upgradeType == 1 /* ConstructedCardModel.UpgradeType.Add */)
                found = true;
        }

        Assert.True(found, "Expected TheFinalLesson to queue CardKeyword.Innate with UpgradeType.Add.");
    }
}
