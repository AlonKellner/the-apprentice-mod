using System.Collections;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// ConstructedUpgrade() (which actually applies UpgradeKeywords) only runs via a Harmony transpiler
// patch that isn't active in this bare xUnit project (same boundary AssemblyInfo.cs documents for
// CardModifier.AddModifier<T>), so these tests assert the declaration recorded at construction time
// rather than the runtime effect of calling .Upgrade(). Real upgrade behavior is /verify-only.
public class TheFirstLessonTests
{
    [Fact]
    public void TheFirstLesson_BaseCard_DoesNotHaveInnate()
    {
        var card = new TheFirstLesson();
        Assert.DoesNotContain(CardKeyword.Innate, card.Keywords);
    }

    [Fact]
    public void TheFirstLesson_QueuesInnateKeywordToAddOnUpgrade()
    {
        var card = new TheFirstLesson();
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

        Assert.True(found, "Expected TheFirstLesson to queue CardKeyword.Innate with UpgradeType.Add.");
    }
}
