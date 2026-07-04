using System.Collections;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

public class MyOwnLessonTests
{
    [Fact]
    public void MyOwnLesson_HasExpectedShape()
    {
        var card = new MyOwnLesson();
        Assert.Equal("TheUnderstudy:MyOwnLesson", MyOwnLesson.CardId);
        Assert.Equal(CardType.Power, card.Type);
        Assert.Equal(CardRarity.Rare, card.Rarity);
        Assert.Equal(TargetType.None, card.TargetType);
    }

    [Fact]
    public void MyOwnLesson_BaseCard_DoesNotHaveInnate()
    {
        var card = new MyOwnLesson();
        Assert.DoesNotContain(CardKeyword.Innate, card.Keywords);
    }

    [Fact]
    public void MyOwnLesson_QueuesInnateKeywordToAddOnUpgrade()
    {
        var card = new MyOwnLesson();
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

        Assert.True(found, "Expected MyOwnLesson to queue CardKeyword.Innate with UpgradeType.Add.");
    }
}
