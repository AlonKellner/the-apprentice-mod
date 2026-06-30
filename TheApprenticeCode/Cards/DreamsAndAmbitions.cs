using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public static class DreamsAndAmbitions
{
    private static ICombatState? _lastCombat;
    private static int _dreamsCreated;
    private static int _ambitionsCreated;

    private static void MaybeResetForCombat(ICombatState combat)
    {
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _dreamsCreated = 0;
            _ambitionsCreated = 0;
        }
    }

    public static int CountDreams(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Dream);

    public static int CountAmbitions(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Ambition);

    public static int CountAll(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Dream || c is Ambition);

    public static int CountPotentials(IEnumerable<CardModel> cards) =>
        cards.Count(c => c is Potential);

    public static void UpdateDreamyCards(IEnumerable<CardModel> allCards, int newTotal)
    {
        foreach (var card in allCards)
            if (card.TryGetModifier<DreamyModifier>(out var mod))
                card.DynamicVars.Block.BaseValue = mod.OriginalBaseBlock + newTotal;
    }

    public static void UpdateAmbitousCards(IEnumerable<CardModel> allCards, int newTotal)
    {
        foreach (var card in allCards)
            if (card.TryGetModifier<AmbitousModifier>(out var mod))
                card.DynamicVars.Damage.BaseValue = mod.OriginalBaseDamage + newTotal;
    }

    private static void ApplyDreamyModifier(CardModel card)
    {
        int origBlock = (int)card.DynamicVars.Block.BaseValue;
        CardModifier.AddModifier<DreamyModifier>(card);
        if (card.TryGetModifier<DreamyModifier>(out var mod))
            mod.OriginalBaseBlock = origBlock;
    }

    private static void ApplyAmbitousModifier(CardModel card)
    {
        int origDamage = (int)card.DynamicVars.Damage.BaseValue;
        CardModifier.AddModifier<AmbitousModifier>(card);
        if (card.TryGetModifier<AmbitousModifier>(out var mod))
            mod.OriginalBaseDamage = origDamage;
    }

    private static void ApplyExpendModifier(CardModel card)
    {
        CardModifier.AddModifier<ExpendModifier>(card);
    }

    public static async Task AddDreams(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        MaybeResetForCombat(combatState);
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Dream>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            ApplyDreamyModifier(card);
            ApplyExpendModifier(card);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
        _dreamsCreated += count;
        UpdateDreamyCards(player.Piles.SelectMany(p => p.Cards).ToList(), _dreamsCreated);
    }

    public static async Task AddAmbitions(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        MaybeResetForCombat(combatState);
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Ambition>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            ApplyAmbitousModifier(card);
            ApplyExpendModifier(card);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
        _ambitionsCreated += count;
        UpdateAmbitousCards(player.Piles.SelectMany(p => p.Cards).ToList(), _ambitionsCreated);
    }

    public static async Task AddPotentials(Player player, ICombatState combatState, int count, bool upgraded = false)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        MaybeResetForCombat(combatState);
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<Potential>(player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            ApplyDreamyModifier(card);
            ApplyAmbitousModifier(card);
            ApplyExpendModifier(card);
            cards.Add(card);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        UpdateDreamyCards(allCards, _dreamsCreated);
        UpdateAmbitousCards(allCards, _ambitionsCreated);
    }
}
