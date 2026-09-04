using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Adds selection-order badges to the in-hand selection mode (NPlayerHand.SelectCards), gated to Planned
// selections the same way as the grid patch. Badges are driven off the hand's authoritative
// _selectedCards list (click order, updated synchronously on every select/deselect) rather than the
// lifted-holder row, because the row lags by one deselect (see SelectedCardsRef) — which used to leave a
// stale badge on a just-deselected card until the next selection replaced it.
[HarmonyPatch]
public static class PlannedHandSelectionPatch
{
    // Presence == "this hand is mid Planned selection, badge it".
    private static readonly ConditionalWeakTable<NPlayerHand, object> Tagged = new();
    private static readonly object Marker = new();

    private static readonly AccessTools.FieldRef<NPlayerHand, NSelectedHandCardContainer> ContainerRef =
        AccessTools.FieldRefAccess<NPlayerHand, NSelectedHandCardContainer>("_selectedHandCardContainer");

    // The authoritative, click-ordered selection list. It is updated SYNCHRONOUSLY inside both
    // SelectCardInSimpleMode (Add) and DeselectCard (Remove), so it is correct by the time our postfixes
    // run. container.Holders is NOT: NSelectedHandCardContainer.DeselectHolder calls Hand.DeselectCard
    // first and only removes the lifted holder AFTER that returns, so the just-deselected card lingers in
    // Holders through this postfix — which is exactly why its badge used to survive until the next select.
    private static readonly AccessTools.FieldRef<NPlayerHand, List<CardModel>> SelectedCardsRef =
        AccessTools.FieldRefAccess<NPlayerHand, List<CardModel>>("_selectedCards");

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

    // Combat state can change mid-selection (e.g. an ally kills a monster whose power was downgrading our
    // cards), and the base game silently deselects anything that no longer qualifies WITHOUT going through
    // DeselectCard. Re-render here too so those forced deselects don't strand a badge.
    [HarmonyPatch(typeof(NPlayerHand), "RevalidateSelectionAfterStateChange")]
    [HarmonyPostfix]
    public static void RevalidatePostfix(NPlayerHand __instance)
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

        // Map each lifted card's model -> its live NCard node. Membership/order do NOT come from here
        // (see SelectedCardsRef): container.Holders can still carry a just-deselected holder this frame.
        var nodeByModel = new Dictionary<CardModel, NCard>();
        var containerModels = new List<CardModel>();
        foreach (var h in container.Holders)
        {
            var node = h.CardNode;
            if (node?.Model is { } m)
            {
                nodeByModel[m] = node;
                containerModels.Add(m);
            }
        }

        // Authoritative selection + click order. A deselected card is already gone from here, so it drops
        // out of the badge set even though its lifted holder may still be in container.Holders.
        var selected = SelectedCardsRef(hand);
        var orderedNodes = new List<NCard>();
        foreach (var model in selected)
            if (nodeByModel.TryGetValue(model, out var node)) orderedNodes.Add(node);

        // Offset by the plan slots already on the board so the badge shows the real Planned #N.
        int firstNumber = orderedNodes.Count == 0
            ? 1
            : PlannedModifier.TotalSlotCount(PlannedModifier.RelevantCards(orderedNodes[0].Model?.Owner)) + 1;

        var items = new List<(NCard card, int number)>();
        for (int i = 0; i < orderedNodes.Count; i++)
            items.Add((orderedNodes[i], firstNumber + i));
        SelectionIndexBadge.Render(items);

        // Publish the click order (from the authoritative selection) so single-player appliers assign
        // Planned slots in it (no-op in MP).
        var orderedModels = orderedNodes.Select(n => n.Model).Where(m => m != null).Cast<CardModel>().ToList();
        PlannedSelectionState.PublishClickOrder(orderedModels, orderedModels.Count > 0 ? orderedModels[0].Owner : null);

        SelectionIndexBadge.DiagAfterRender("hand", selected, containerModels, orderedModels);
    }
}
