using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards.Modifiers;

public class PlannedModifier : CardModifier
{
    public const string ModifierId = "TheApprentice:Planned";

    public int SequenceIndex { get; set; }
    public int VisualIndex { get; set; }

    public static event Action? Changed;

    // Computes next sequence index (max existing + 1). Pre-Planned cards at -1
    // are naturally excluded since any user-planned card at >= 0 wins the max.
    // Returns 0 when no Planned cards exist.
    public static bool CanApplyTo(CardModel card) =>
        !card.TryGetModifier<PlannedModifier>(out _) && !card.IsUnplayable();

    public static bool AnyIn(IEnumerable<CardModel> cards) =>
        cards.Any(c => c.TryGetModifier<PlannedModifier>(out _));

    public static int CountIn(IEnumerable<CardModel> cards) =>
        cards.Count(c => c.TryGetModifier<PlannedModifier>(out _));

    public static List<(CardModel card, PlannedModifier mod)> GetSorted(IEnumerable<CardModel> cards)
    {
        var indexed = new List<(CardModel card, PlannedModifier mod, int deckIdx)>();
        int i = 0;
        foreach (var c in cards)
        {
            if (c.TryGetModifier<PlannedModifier>(out var mod))
                indexed.Add((c, mod, i));
            i++;
        }
        indexed.Sort((a, b) =>
        {
            int cmp = a.mod.SequenceIndex.CompareTo(b.mod.SequenceIndex);
            return cmp != 0 ? cmp : a.deckIdx.CompareTo(b.deckIdx);
        });
        return indexed.Select(x => (x.card, x.mod)).ToList();
    }

    public static void AssignVisualIndices(List<(CardModel card, PlannedModifier mod)> sorted)
    {
        for (int i = 0; i < sorted.Count; i++)
            sorted[i].mod.VisualIndex = i + 1;
    }

    public static void RefreshVisualIndices(IEnumerable<CardModel> allCards)
    {
        AssignVisualIndices(GetSorted(allCards));
    }

    public static void Apply(CardModel card, IEnumerable<CardModel> allCards)
    {
        int max = -1;
        foreach (var c in allCards)
            if (c.TryGetModifier<PlannedModifier>(out var existing) && existing.SequenceIndex > max)
                max = existing.SequenceIndex;
        CardModifier.AddModifier<PlannedModifier>(card);
        if (card.TryGetModifier<PlannedModifier>(out var mod))
            mod.SequenceIndex = max + 1;
        Changed?.Invoke();
    }

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner)
        {
            keywords.Add(ApprenticeKeywords.Planned);
            bool virtuosoActive = card.Owner?.Creature?.Powers.OfType<VirtuosoPower>().Any() ?? false;
            if (!virtuosoActive)
                keywords.Add(CardKeyword.Unplayable);
            return true;
        }
        return false;
    }

    public override void ModifyDescriptionPost(Creature? creature, ref string description)
    {
        int display = VisualIndex > 0 ? VisualIndex : SequenceIndex + 1;
        description += $"\n[gold]Planned[/gold] #{display}.";
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.IntProperties["seq"] = SequenceIndex;
    }

    public override void LoadSaveData(ModifierSave save)
    {
        if (save.IntProperties.TryGetValue("seq", out int seq))
            SequenceIndex = seq;
    }
}
