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
}
