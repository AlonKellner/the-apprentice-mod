using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheApprentice.TheApprenticeCode.Cards;

namespace TheApprentice.TheApprenticeCode.Patches;

// Reorders afterDescription so Expend appears before Exhaust.
// BaseLib's AutoKeywordText postfix appends AdditionalAfterKeywords after [Exhaust, Eternal],
// producing [Exhaust, Eternal, Expend]. Our postfix runs after BaseLib's and moves Expend first.
[HarmonyPatch(typeof(CardKeywordOrder), MethodType.StaticConstructor)]
public static class ExpendKeywordOrderPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref CardKeyword[] ___afterDescription)
    {
        var list = ___afterDescription.ToList();
        int expendIdx = list.IndexOf(ApprenticeKeywords.Expend);
        int exhaustIdx = list.IndexOf(CardKeyword.Exhaust);
        if (expendIdx > exhaustIdx && expendIdx >= 0 && exhaustIdx >= 0)
        {
            list.RemoveAt(expendIdx);
            list.Insert(exhaustIdx, ApprenticeKeywords.Expend);
            ___afterDescription = list.ToArray();
        }
    }
}
