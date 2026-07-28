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

// TEMP DIAGNOSTIC: Dusty Tome crashes granting One Take (AncientCard null). Log the exact state after
// SetupForPlayer so we can tell WHY: is One Take in ModelDb.AllCards (what the BaseLib DustyTomePatch
// scans)? Is it recognised as ITomeCard? What ancient cards does the pool's GetUnlockedCards vs
// ModelDb.AllCards actually contain, and did AncientCard get set? Remove once diagnosed.
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
