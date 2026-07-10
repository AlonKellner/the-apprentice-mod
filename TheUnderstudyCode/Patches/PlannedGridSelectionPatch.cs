using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Adds selection-order badges to the draw/discard grid selection screen (NCombatPileCardSelectScreen),
// but only when a Planned applier armed the selection (see PlannedSelectionState). The base screen
// stores selections in an unordered HashSet, so we keep our own click-ordered list per screen; that
// same order is published on completion so PlannedModifier.Apply assigns slots in click order.
[HarmonyPatch]
public static class PlannedGridSelectionPatch
{
    // Presence in this table == "this screen is a Planned selection, badge it". The list is the click
    // order. ConditionalWeakTable keys weakly, so a freed screen drops out without leaking.
    private static readonly ConditionalWeakTable<NCombatPileCardSelectScreen, List<CardModel>> Orders = new();

    private static readonly AccessTools.FieldRef<NCombatPileCardSelectScreen, HashSet<CardModel>> SelectedCardsRef =
        AccessTools.FieldRefAccess<NCombatPileCardSelectScreen, HashSet<CardModel>>("_selectedCards");

    // _grid is a protected field declared on the base NCardGridSelectionScreen; AccessTools.Field walks
    // the hierarchy, so requesting it off the derived type resolves the inherited field.
    private static readonly AccessTools.FieldRef<NCombatPileCardSelectScreen, NCardGrid> GridRef =
        AccessTools.FieldRefAccess<NCombatPileCardSelectScreen, NCardGrid>("_grid");

    // Create runs synchronously inside CardSelectCmd.FromCombatPile before any await, so the armed flag
    // an applier just set is still readable here. Consume it and tag the new screen.
    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), nameof(NCombatPileCardSelectScreen.Create))]
    [HarmonyPostfix]
    public static void CreatePostfix(NCombatPileCardSelectScreen __result)
    {
        if (!PlannedSelectionState.ConsumeArmed()) return;
        // Clear any badges stranded by a previous selection that closed without completing (cancel).
        SelectionIndexBadge.ClearAll();
        Orders.AddOrUpdate(__result, new List<CardModel>());
    }

    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), "OnCardClicked")]
    [HarmonyPostfix]
    public static void OnCardClickedPostfix(NCombatPileCardSelectScreen __instance, CardModel card)
    {
        if (!Orders.TryGetValue(__instance, out var order)) return;
        var selected = SelectedCardsRef(__instance);

        if (selected.Contains(card))
        {
            if (!order.Contains(card)) order.Add(card);
        }
        else
        {
            order.Remove(card);
        }
        // The base screen can silently drop selections (e.g. at MaxSelect); keep our order in sync.
        order.RemoveAll(c => !selected.Contains(c));
        RenderBadges(__instance, order);
    }

    // The grid rebuilds its card nodes and re-highlights here whenever the pile changes, so re-attach
    // badges onto the fresh nodes. (If this call itself completed the selection, CompleteSelectionPrefix
    // already untagged the screen and TryGetValue returns false, so we don't fight it.)
    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), "UpdatePileContents")]
    [HarmonyPostfix]
    public static void UpdatePileContentsPostfix(NCombatPileCardSelectScreen __instance)
    {
        if (!Orders.TryGetValue(__instance, out var order)) return;
        var selected = SelectedCardsRef(__instance);
        order.RemoveAll(c => !selected.Contains(c));
        foreach (var c in selected)
            if (!order.Contains(c)) order.Add(c);
        RenderBadges(__instance, order);
    }

    // Runs before the base method sets the result and closes the screen: publish the final click order
    // so the applier queues Planned in it, and untag so later postfixes (e.g. UpdatePileContents fired
    // from the same call) skip.
    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), "CompleteSelection")]
    [HarmonyPrefix]
    public static void CompleteSelectionPrefix(NCombatPileCardSelectScreen __instance)
    {
        if (!Orders.TryGetValue(__instance, out var order)) return;
        var selected = SelectedCardsRef(__instance);
        PlannedSelectionState.PublishGridOrder(order.Where(c => selected.Contains(c)).ToList());
        SelectionIndexBadge.ClearAll();
        Orders.Remove(__instance);
    }

    private static void RenderBadges(NCombatPileCardSelectScreen screen, List<CardModel> order)
    {
        var grid = GridRef(screen);
        // Offset by the plan slots already on the board so the badge shows the real Planned #N the card
        // will display, not just its 1..N position within this selection.
        int firstNumber = order.Count == 0
            ? 1
            : PlannedModifier.TotalSlotCount(PlannedModifier.RelevantCards(order[0].Owner)) + 1;

        var items = new List<(NCard card, int number)>();
        for (int i = 0; i < order.Count; i++)
            if (grid.GetCardNode(order[i]) is { } node)
                items.Add((node, firstNumber + i));
        SelectionIndexBadge.Render(items);
    }
}
