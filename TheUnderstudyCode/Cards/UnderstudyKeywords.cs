using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public static class UnderstudyKeywords
{
    [CustomEnum]
    public static CardKeyword Planned;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Tuned;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Stable;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Invert;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Invertible;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Swap;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Swappable;

    // The simultaneous-resolution keyword for cards that do both at once (Best of Both / Pass the Mic /
    // Standing Ovation): a debuff that is both swappable AND invertible is traded to the enemies AND its
    // buff is gained, from the same snapshot, instead of one consuming it before the other.
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword SwapAndInvert;
}
