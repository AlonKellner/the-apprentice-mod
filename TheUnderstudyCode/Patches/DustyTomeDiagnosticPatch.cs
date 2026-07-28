using System.Linq;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The DustyTome instance obtained from the ancient node is NOT the one SetupForPlayer previewed on:
// AncientCard is a [SavedProperty] (ModelId?) that comes back null on the freshly-created obtained
// instance, so AfterObtained crashes on ModelDb.GetById(null). Diagnosed via DustyTomeDiagnosticPatch:
// SetupForPlayer runs fine and picks One Take, but the obtained instance never carried the value.
// If AncientCard is unset when the relic is obtained, set it up now — SetupForPlayer routes through
// BaseLib's ITomeCard path and picks One Take for the Understudy. Guarded on null so base-game
// characters (whose obtained instance already carries a chosen ancient) are untouched.
[HarmonyPatch(typeof(DustyTome), "AfterObtained")]
public static class DustyTomeObtainFix
{
    [HarmonyPrefix]
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.AncientCard == null && __instance.Owner != null)
            __instance.SetupForPlayer(__instance.Owner);
    }
}

// TEMP DIAGNOSTIC: kept one more round to confirm the fix — logs AncientCard after each SetupForPlayer.
[HarmonyPatch(typeof(DustyTome), "SetupForPlayer")]
public static class DustyTomeDiagnosticPatch
{
    [HarmonyPostfix]
    public static void Postfix(DustyTome __instance, Player player)
    {
        var allCards = ModelDb.AllCards.ToList();
        var oneTake = ModelDb.Card<OneTake>();
        bool inAllCards = allCards.Any(c => c.Id.Equals(oneTake.Id));
        bool isTome = oneTake is ITomeCard;

        var ancientInAllCards = allCards.Where(c => c.Rarity == CardRarity.Ancient)
            .Select(c => c.Id.ToString());
        var ancientInPool = player.Character.CardPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity == CardRarity.Ancient).Select(c => c.Id.ToString());
        var ancientId = Traverse.Create(__instance).Property("AncientCard").GetValue();

        Log.Info($"[TomeDiag] AncientCard={ancientId?.ToString() ?? "null"}; " +
                 $"OneTake: inAllCards={inAllCards} isITomeCard={isTome} rarity={oneTake.Rarity}; " +
                 $"ancientInAllCards=[{string.Join(", ", ancientInAllCards)}]; " +
                 $"ancientInPool=[{string.Join(", ", ancientInPool)}]");
    }
}
