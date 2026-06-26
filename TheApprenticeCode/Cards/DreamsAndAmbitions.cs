using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheApprentice.TheApprenticeCode.Cards;

public static class DreamsAndAmbitions
{
    public static int CountDreams(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Dream);

    public static int CountAmbitions(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Ambition);

    public static int CountAll(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Dream || c is Ambition);

    public static int CountPotentials(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Potential);

    // Mirrors the Shiv.CreateInHand pattern: combatState.CreateCard<T> for runtime instantiation.
    public static async Task AddDreams(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Dream>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
    }

    public static async Task AddAmbitions(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Ambition>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
    }

    public static async Task AddPotentials(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Potential>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
    }
}
