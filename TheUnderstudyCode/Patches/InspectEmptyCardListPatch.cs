using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Base-game crash guard. NCardGridSelectionScreen.ShowCardDetail opens the card-inspect screen with
// `Open(_cards.ToList(), _cards.IndexOf(card), ...)`. When the selection's selectable-card list is empty
// (e.g. alt-clicking to inspect a card in a Magnum Opus / Unwind / Workshop selection grid where no
// card currently qualifies), `_cards` is empty, so NInspectCardScreen.SetCard runs
// `Math.Clamp(index, 0, _cards.Count - 1)` = Clamp(_, 0, -1) and throws ArgumentException
// ("'0' cannot be greater than -1"). Inspecting an empty list is meaningless, so skip the open entirely.
// This is a general base-game gap our selection-heavy cards surface often; guarding Open fixes it for
// every caller and leaves the inspect screen simply closed.
[HarmonyPatch(typeof(NInspectCardScreen), nameof(NInspectCardScreen.Open),
    new[] { typeof(List<CardModel>), typeof(int), typeof(bool) })]
public static class InspectEmptyCardListPatch
{
    [HarmonyPrefix]
    public static bool Prefix(List<CardModel> cards) => cards is { Count: > 0 };
}
