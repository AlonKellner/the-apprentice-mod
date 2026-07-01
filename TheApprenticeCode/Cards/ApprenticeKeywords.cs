using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TheApprentice.TheApprenticeCode.Cards;

public static class ApprenticeKeywords
{
    [CustomEnum]
    public static CardKeyword Planned;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Dreamy;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Ambitous;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Expend;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Intense;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Stable;
}
