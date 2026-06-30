using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Patches;
using Xunit;

namespace TheApprentice.Tests.Patches;

public class UnplayableKeywordOrderPatchTests
{
    [Fact]
    public void Postfix_MovesUnplayableToEnd_WhenCustomBeforeKeywordsFollowIt()
    {
        // Mirrors the real runtime shape: base array [..., Unplayable] followed by
        // BaseLib-appended custom AutoKeywordPosition.Before keywords (Dreamy, Ambitous).
        var array = new[]
        {
            CardKeyword.Ethereal, CardKeyword.Sly, CardKeyword.Retain,
            CardKeyword.Innate, CardKeyword.Unplayable,
            ApprenticeKeywords.Dreamy, ApprenticeKeywords.Ambitous,
        };

        UnplayableKeywordOrderPatch.Postfix(ref array);

        Assert.Equal(CardKeyword.Unplayable, array.Last());
    }

    [Fact]
    public void Postfix_NoOp_WhenUnplayableAlreadyLast()
    {
        var array = new[] { CardKeyword.Ethereal, ApprenticeKeywords.Dreamy, CardKeyword.Unplayable };
        var original = array.ToArray();

        UnplayableKeywordOrderPatch.Postfix(ref array);

        Assert.Equal(original, array);
    }
}
