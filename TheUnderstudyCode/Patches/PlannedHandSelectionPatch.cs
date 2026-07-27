using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Adds selection-order badges to the in-hand selection mode (NPlayerHand.SelectCards), gated to Planned
// selections the same way as the grid patch. The hand already lifts selected cards into an ordered row
// (_selectedHandCardContainer.Holders, in click order), so there's no ordering to capture here — we
// just draw the number badge onto each lifted card whenever the selection changes.
[HarmonyPatch]
public static class PlannedHandSelectionPatch
{
    // Presence == "this hand is mid Planned selection, badge it".
    private static readonly ConditionalWeakTable<NPlayerHand, object> Tagged = new();
    private static readonly object Marker = new();

    private static readonly AccessTools.FieldRef<NPlayerHand, NSelectedHandCardContainer> ContainerRef =
        AccessTools.FieldRefAccess<NPlayerHand, NSelectedHandCardContainer>("_selectedHandCardContainer");

    // SelectCards sets up select mode synchronously before awaiting, so the armed flag is still set.
    [HarmonyPatch(typeof(NPlayerHand), nameof(NPlayerHand.SelectCards))]
    [HarmonyPrefix]
    public static void SelectCardsPrefix(NPlayerHand __instance)
    {
        if (!PlannedSelectionState.ConsumeArmed()) return;
        SelectionIndexBadge.ClearAll();
        Tagged.AddOrUpdate(__instance, Marker);
    }

    [HarmonyPatch(typeof(NPlayerHand), "SelectCardInSimpleMode")]
    [HarmonyPostfix]
    public static void SelectCardPostfix(NPlayerHand __instance)
    {
        if (Tagged.TryGetValue(__instance, out _)) RenderBadges(__instance);
    }

    [HarmonyPatch(typeof(NPlayerHand), nameof(NPlayerHand.DeselectCard))]
    [HarmonyPostfix]
    public static void DeselectCardPostfix(NPlayerHand __instance)
    {
        if (Tagged.TryGetValue(__instance, out _)) RenderBadges(__instance);
    }

    // AfterCardsSelected is where select mode ends (_selectedCards.Clear()); tear the badges down and
    // untag so nothing lingers on cards as they slide back into hand.
    [HarmonyPatch(typeof(NPlayerHand), "AfterCardsSelected")]
    [HarmonyPostfix]
    public static void AfterCardsSelectedPostfix(NPlayerHand __instance)
    {
        if (!Tagged.TryGetValue(__instance, out _)) return;
        SelectionIndexBadge.ClearAll();
        Tagged.Remove(__instance);
    }

    private static void RenderBadges(NPlayerHand hand)
    {
        var container = ContainerRef(hand);
        var nodes = container.Holders
            .Select(h => h.CardNode)
            .Where(n => n != null)
            .Cast<NCard>()
            .ToList();
        // Offset by the plan slots already on the board so the badge shows the real Planned #N.
        int firstNumber = nodes.Count == 0
            ? 1
            : PlannedModifier.TotalSlotCount(PlannedModifier.RelevantCards(nodes[0].Model?.Owner)) + 1;

        var items = new List<(NCard card, int number)>();
        for (int i = 0; i < nodes.Count; i++)
            items.Add((nodes[i], firstNumber + i));
        SelectionIndexBadge.Render(items);
    }
}
