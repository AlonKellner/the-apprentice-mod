using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Hunter Killer's TenderPower takes -1 Strength / -1 Dexterity per card you play and, at the end of your
// turn, gives back one of each per card played, however much it actually took. Inverting the loss
// mid-turn (or My Own Lesson turning it into a gain, or Pulled Punch softening it) still gets the full
// amount back, so you end the turn with a permanent stat gain.
//
// Fix: record what each card actually took (the stat before vs after Tender's own apply) as a per-stat
// debt in RestoreDebts, let Invert pay that debt off (RestoreDebts.Forgive), and give back only the debt
// that is left at turn end.
[HarmonyPatch(typeof(TenderPower), nameof(TenderPower.AfterCardPlayed))]
public static class TenderDebtRecordPatch
{
    [HarmonyPrefix]
    public static void Prefix(TenderPower __instance, CardPlay cardPlay, out (int Str, int Dex)? __state)
    {
        __state = null;
        if (!__instance.IsMutable || cardPlay.Card.Owner != __instance.Owner.Player) return;
        __state = (__instance.Owner.GetPowerAmount<StrengthPower>(), __instance.Owner.GetPowerAmount<DexterityPower>());
    }

    [HarmonyPostfix]
    public static void Postfix(TenderPower __instance, (int Str, int Dex)? __state, ref Task __result)
    {
        if (__state is not { } before) return;
        __result = RecordAfter(__result, __instance, before);
    }

    private static async Task RecordAfter(Task original, TenderPower tender, (int Str, int Dex) before)
    {
        await original;
        var debt = RestoreDebts.TenderDebt(tender);
        debt.Strength += RestoreDebts.LandedLoss(before.Str, tender.Owner.GetPowerAmount<StrengthPower>());
        debt.Dexterity += RestoreDebts.LandedLoss(before.Dex, tender.Owner.GetPowerAmount<DexterityPower>());
    }
}

[HarmonyPatch(typeof(TenderPower), nameof(TenderPower.AfterSideTurnEnd))]
public static class TenderDebtRestorePatch
{
    private static readonly System.Reflection.MethodInfo SetCardsPlayedThisTurn =
        AccessTools.PropertySetter(typeof(TenderPower), "CardsPlayedThisTurn");

    [HarmonyPrefix]
    public static bool Prefix(TenderPower __instance, PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants, ref Task __result)
    {
        if (!__instance.IsMutable) return true;
        __result = Restore(__instance, choiceContext, participants);
        return false;
    }

    // The base method, giving back the remaining debt instead of the card count.
    private static async Task Restore(TenderPower tender, PlayerChoiceContext choiceContext, IEnumerable<Creature> participants)
    {
        if (!System.Linq.Enumerable.Contains(participants, tender.Owner)) return;

        var debt = RestoreDebts.TenderDebt(tender);
        int strength = debt.Strength, dexterity = debt.Dexterity;
        debt.Strength = debt.Dexterity = 0;

        if (strength > 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, tender.Owner, strength, tender.Applier, null, silent: true);
        if (dexterity > 0)
            await PowerCmd.Apply<DexterityPower>(choiceContext, tender.Owner, dexterity, tender.Applier, null, silent: true);

        // Through the base (private) setter, so the displayed counter refreshes like the base reset.
        SetCardsPlayedThisTurn.Invoke(tender, new object[] { 0 });
    }
}
