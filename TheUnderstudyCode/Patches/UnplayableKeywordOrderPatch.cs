using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Reorders beforeDescription so Unplayable appears at the START of the before-badge
// block. CardModel's renderer Inserts each matched keyword at index 0 in array order,
// so the LAST array entry ends up FIRST/frontmost. BaseLib's AutoKeywordText postfix
// appends custom AutoKeywordPosition.Before keywords (Tense, Stable) after the base
// array's Unplayable entry, pushing Unplayable out of last place. Our postfix runs
// after BaseLib's and moves Unplayable back to the end, regardless of whether it was
// added by TenseModifier or PlannedModifier.
[HarmonyPatch(typeof(CardKeywordOrder), MethodType.StaticConstructor)]
public static class UnplayableKeywordOrderPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref CardKeyword[] ___beforeDescription)
    {
        var list = ___beforeDescription.ToList();
        int idx = list.IndexOf(CardKeyword.Unplayable);
        if (idx >= 0 && idx != list.Count - 1)
        {
            list.RemoveAt(idx);
            list.Add(CardKeyword.Unplayable);
            ___beforeDescription = list.ToArray();
        }
    }
}
