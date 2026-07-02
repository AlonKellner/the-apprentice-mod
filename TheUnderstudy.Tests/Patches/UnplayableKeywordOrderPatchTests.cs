using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Patches;

public class UnplayableKeywordOrderPatchTests
{
    [Fact]
    public void Postfix_MovesUnplayableToEnd_WhenCustomBeforeKeywordsFollowIt()
    {
        // Mirrors the real runtime shape: base array [..., Unplayable] followed by
        // BaseLib-appended custom AutoKeywordPosition.Before keywords (Intense, Stable).
        var array = new[]
        {
            CardKeyword.Ethereal, CardKeyword.Sly, CardKeyword.Retain,
            CardKeyword.Innate, CardKeyword.Unplayable,
            UnderstudyKeywords.Intense, UnderstudyKeywords.Stable,
        };

        UnplayableKeywordOrderPatch.Postfix(ref array);

        Assert.Equal(CardKeyword.Unplayable, array.Last());
    }

    [Fact]
    public void Postfix_NoOp_WhenUnplayableAlreadyLast()
    {
        var array = new[] { CardKeyword.Ethereal, UnderstudyKeywords.Intense, CardKeyword.Unplayable };
        var original = array.ToArray();

        UnplayableKeywordOrderPatch.Postfix(ref array);

        Assert.Equal(original, array);
    }
}
