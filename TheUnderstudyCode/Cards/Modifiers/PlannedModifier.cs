using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

public class PlannedModifier : CardModifier
{
    public const string ModifierId = "TheUnderstudy:Planned";

    // BaseLib clones modifier instances from a prototype via shallow copy, so auto-property
    // initializers (= new()) produce shared collection references across all clones. We use
    // private backing fields that can be reassigned by ReinitCollections() to break sharing.
    private List<int> _sequenceIndices = new();
    private Dictionary<int, int> _visualBySeq = new();

    // One entry per Planned "slot" this card occupies in the queue.
    // A card may be Planned #4 and #6 simultaneously by having two entries here.
    public List<int> SequenceIndices => _sequenceIndices;

    // Maps seqIdx → 1-based visual display number. Populated by AssignVisualIndices.
    public Dictionary<int, int> VisualBySeq => _visualBySeq;

    // Replaces the collection instances with fresh ones, breaking any prototype sharing
    // introduced by BaseLib's shallow-clone modifier creation. Called in Apply whenever
    // a new modifier is attached so each card gets its own independent list and dict.
    public void ReinitCollections()
    {
        _sequenceIndices = new List<int>();
        _visualBySeq = new Dictionary<int, int>();
    }

    public static event Action? Changed;

    // Player.Piles includes the Deck pile (the persistent, between-combats card list) alongside
    // the real combat piles (Draw/Hand/Discard/Exhaust/Play). Combat start clones every Deck card
    // into the draw pile rather than moving it, so a card present in the deck still shows up in
    // Piles after combat begins — counting/sorting it alongside its combat-pile clone would
    // double the apparent count and skew sequencing. Use this wherever "all cards relevant to
    // Planned tracking this combat" is needed.
    public static IEnumerable<CardModel> RelevantCards(Player? player) =>
        (player?.Piles ?? Enumerable.Empty<CardPile>())
            .Where(p => p.Type.IsCombatPile())
            .SelectMany(p => p.Cards);

    // Attacks and Skills can be Planned (Powers, Statuses, Curses, and Quests cannot).
    // Stacking is allowed: a card already Planned can be Planned again for a second queue slot.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill)
        && !card.Keywords.Contains(UnderstudyKeywords.Stable);

    public static bool AnyIn(IEnumerable<CardModel> cards) =>
        cards.Any(c => c.TryGetModifier<PlannedModifier>(out _));

    public static int CountIn(IEnumerable<CardModel> cards) =>
        cards.Count(c => c.TryGetModifier<PlannedModifier>(out _));

    // Returns one entry per queue slot. A card with two slots appears twice.
    // Sorted by slotSeqIdx ascending, then by deck position as a tiebreaker.
    public static List<(CardModel card, PlannedModifier mod, int slotSeqIdx)> GetSorted(IEnumerable<CardModel> cards)
    {
        var entries = new List<(CardModel card, PlannedModifier mod, int slotSeqIdx, int deckIdx)>();
        int i = 0;
        foreach (var c in cards)
        {
            if (c.TryGetModifier<PlannedModifier>(out var mod))
                foreach (var seqIdx in mod.SequenceIndices)
                    entries.Add((c, mod, seqIdx, i));
            i++;
        }
        entries.Sort((a, b) =>
        {
            int cmp = a.slotSeqIdx.CompareTo(b.slotSeqIdx);
            return cmp != 0 ? cmp : a.deckIdx.CompareTo(b.deckIdx);
        });
        return entries.Select(x => (x.card, x.mod, x.slotSeqIdx)).ToList();
    }

    public static void AssignVisualIndices(List<(CardModel card, PlannedModifier mod, int slotSeqIdx)> sorted)
    {
        foreach (var (_, mod, _) in sorted) mod.VisualBySeq.Clear();
        for (int i = 0; i < sorted.Count; i++)
        {
            var (_, mod, slotSeqIdx) = sorted[i];
            mod.VisualBySeq[slotSeqIdx] = i + 1;
        }
    }

    public static void RefreshVisualIndices(IEnumerable<CardModel> allCards)
    {
        AssignVisualIndices(GetSorted(allCards));
    }

    // Appends a new queue slot to the card. Creates the modifier if the card doesn't have one yet.
    public static void Apply(CardModel card, IEnumerable<CardModel> allCards)
    {
        int max = -1;
        foreach (var c in allCards)
            if (c.TryGetModifier<PlannedModifier>(out var existing))
                foreach (var s in existing.SequenceIndices)
                    if (s > max) max = s;
        if (!card.TryGetModifier<PlannedModifier>(out var mod))
        {
            CardModifier.AddModifier<PlannedModifier>(card);
            card.TryGetModifier<PlannedModifier>(out mod);
            // BaseLib shallow-clones modifier prototypes, so the new instance shares its
            // SequenceIndices and VisualBySeq with every other clone. ReinitCollections()
            // replaces both backing fields with fresh instances, breaking the sharing.
            // This also discards any stale save data that LoadSaveData may have loaded.
            mod!.ReinitCollections();
        }
        mod!.SequenceIndices.Add(max + 1);
        if (!card.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(card);
        Changed?.Invoke();
    }

    // Removes a specific queue slot. If the slot list empties, removes the modifier entirely.
    public static void RemoveSlot(CardModel card, int slotSeqIdx, IEnumerable<CardModel> allCards)
    {
        if (card.TryGetModifier<PlannedModifier>(out var mod))
        {
            mod.SequenceIndices.Remove(slotSeqIdx);
            if (mod.SequenceIndices.Count == 0)
            {
                CardModifier.DirectModifiers(card).Remove(mod);
                if (card.TryGetModifier<UnplayableModifier>(out var u))
                    CardModifier.DirectModifiers(card).Remove(u);
            }
        }
        RefreshVisualIndices(allCards);
        Changed?.Invoke();
    }

    // Removes the entire modifier (all queue slots). Used by Improvise, TabulaRasa, etc.
    public static void Remove(CardModel card, IEnumerable<CardModel> allCards)
    {
        if (card.TryGetModifier<PlannedModifier>(out var mod))
        {
            CardModifier.DirectModifiers(card).Remove(mod);
            if (card.TryGetModifier<UnplayableModifier>(out var u))
                CardModifier.DirectModifiers(card).Remove(u);
        }
        RefreshVisualIndices(allCards);
        Changed?.Invoke();
    }

    public static void InvokeChanged() => Changed?.Invoke();

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card == Owner)
        {
            keywords.Add(UnderstudyKeywords.Planned);
            return true;
        }
        return false;
    }

    public override void ModifyDescriptionPost(Creature? creature, ref string description)
    {
        if (SequenceIndices.Count == 0) return;
        var labels = SequenceIndices
            .Select(s => VisualBySeq.TryGetValue(s, out int v) ? $"#{v}" : $"#{s + 1}")
            .ToList();
        description += $"\n[gold]Planned {string.Join(", ", labels)}[/gold].";
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.IntProperties["seq_count"] = SequenceIndices.Count;
        for (int i = 0; i < SequenceIndices.Count; i++)
            save.IntProperties[$"seq_{i}"] = SequenceIndices[i];
    }

    public override void LoadSaveData(ModifierSave save)
    {
        // Assign a fresh list rather than clearing, in case this instance was created via
        // BaseLib's shallow clone and still shares its backing list with the prototype.
        _sequenceIndices = new List<int>();
        if (save.IntProperties.TryGetValue("seq_count", out int count))
        {
            for (int i = 0; i < count; i++)
                if (save.IntProperties.TryGetValue($"seq_{i}", out int s))
                    _sequenceIndices.Add(s);
        }
        else if (save.IntProperties.TryGetValue("seq", out int oldSeq))
        {
            // Backward compatibility: saves created before multi-slot support
            _sequenceIndices.Add(oldSeq);
        }
    }
}
