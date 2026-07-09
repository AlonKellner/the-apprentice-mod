using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
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

    // Raised the first time a card receives a Planned slot (not on subsequent re-Plans of the
    // same card) — Master Form's "whenever you apply Planned... that doesn't have Replay" trigger.
    public static event Action<CardModel>? Applied;

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
        (card.Type == CardType.Attack || card.Type == CardType.Skill) && !card.IsStable();

    public static bool AnyIn(IEnumerable<CardModel> cards) =>
        cards.Any(c => c.TryGetModifier<PlannedModifier>(out _));

    public static int CountIn(IEnumerable<CardModel> cards) =>
        cards.Count(c => c.TryGetModifier<PlannedModifier>(out _));

    // Total queue slots across all cards — a card with two slots (e.g. Planned #1 and #3
    // simultaneously) counts as 2 here, unlike CountIn (which counts distinct cards, so that same
    // card counts as 1). This is what PlannedCounterPower's badge number should show, since its own
    // description lists one line per slot (GetSorted), not one line per distinct card.
    public static int TotalSlotCount(IEnumerable<CardModel> cards) =>
        cards.Sum(c => c.TryGetModifier<PlannedModifier>(out var mod) ? mod.SequenceIndices.Count : 0);

    // Whether the queue contains a card that needs the player to pick a single enemy — used by
    // CurtainCall/Performance/Encore to skip the targeting prompt when nothing queued needs it
    // (empty plan, or only AoE/self/no-target cards, e.g. two Defends).
    public static bool QueueNeedsEnemyTarget(IEnumerable<CardModel> cards) =>
        cards.Any(c => c.TryGetModifier<PlannedModifier>(out _) && c.TargetType == TargetType.AnyEnemy);

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
        int totalVisual = sorted.Select(x => x.mod).Distinct().Sum(m => m.VisualBySeq.Count);
        Invariants.CheckEqual(sorted.Count, totalVisual, nameof(PlannedModifier) + "." + nameof(AssignVisualIndices),
            "plan slots vs assigned visual indices");
    }

    public static void RefreshVisualIndices(IEnumerable<CardModel> allCards)
    {
        AssignVisualIndices(GetSorted(allCards));
    }

    // Slot numbers are handed out by this pure, engine-independent sequencer (see
    // PlannedSlotSequencer.cs) rather than by scanning a caller-supplied card snapshot on every
    // call — a snapshot taken even microseconds before a concurrent/nested Apply call can miss an
    // already-applied slot, handing out a duplicate number (this replaced exactly that bug: Refrain
    // and Overexert both ending up on slot 3 in the same combat).
    private static readonly PlannedSlotSequencer _slots = new();

    // Appends a new queue slot to the card. Creates the modifier if the card doesn't have one yet.
    public static void Apply(CardModel card, ICombatState combat)
    {
        int newSlot = _slots.Next(combat, () =>
        {
            int max = -1;
            foreach (var c in RelevantCards(card.Owner))
                if (c.TryGetModifier<PlannedModifier>(out var existing))
                    foreach (var s in existing.SequenceIndices)
                        if (s > max) max = s;
            return max + 1;
        });

        if (!card.TryGetModifier<PlannedModifier>(out var mod))
        {
            CardModifier.AddModifier<PlannedModifier>(card);
            card.TryGetModifier<PlannedModifier>(out mod);
            // BaseLib shallow-clones modifier prototypes, so the new instance shares its
            // SequenceIndices and VisualBySeq with every other clone. ReinitCollections()
            // replaces both backing fields with fresh instances, breaking the sharing.
            // This also discards any stale save data that LoadSaveData may have loaded.
            mod!.ReinitCollections();
            Applied?.Invoke(card);
        }
        mod!.SequenceIndices.Add(newSlot);
        Invariants.Check(mod.SequenceIndices.Distinct().Count() == mod.SequenceIndices.Count,
            nameof(PlannedModifier) + "." + nameof(Apply),
            $"{card.Id} picked up duplicate slot index {newSlot} on itself — SequenceIndices: [{string.Join(",", mod.SequenceIndices)}]");

        // Cross-card diagnostic. PlannedSlotSequencer makes a collision impossible under normal
        // operation, but this is exactly the invariant that silently broke before (Refrain/Overexert
        // both ending up on slot 3) with nothing logging it at the point of failure — only
        // reconstructible after the fact from timing. If the sequencer's state ever desyncs from
        // live card state again (a future caller bypassing Apply, an unanticipated edge case, etc.),
        // this fires immediately and points at the exact slot and cards involved.
        int collisions = RelevantCards(card.Owner).Count(c => c != card
            && c.TryGetModifier<PlannedModifier>(out var other) && other.SequenceIndices.Contains(newSlot));
        Invariants.Check(collisions == 0,
            nameof(PlannedModifier) + "." + nameof(Apply),
            $"slot {newSlot} assigned to {card.Id} is already held by {collisions} other card(s) — " +
            "monotonic counter desynced from live state");

        // Muscle Memory only protects a card that's ALSO Intense — Planned cards in general still
        // become Unplayable as normal.
        bool immuneViaMuscleMemory = card.TryGetModifier<IntenseModifier>(out _)
            && MuscleMemoryPower.IsActive(card.Owner?.Creature);
        if (!immuneViaMuscleMemory && !card.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(card);
        Log.Info($"PlannedModifier.Apply: {card.Id} took slot {newSlot} ({mod.SequenceIndices.Count} slot(s) total on this card)");
        Changed?.Invoke();
    }

    // Removes a specific queue slot. If the slot list empties, removes the modifier entirely.
    public static void RemoveSlot(CardModel card, int slotSeqIdx, IEnumerable<CardModel> allCards)
    {
        if (card.TryGetModifier<PlannedModifier>(out var mod))
        {
            bool removed = mod.SequenceIndices.Remove(slotSeqIdx);
            Invariants.Check(removed, nameof(PlannedModifier) + "." + nameof(RemoveSlot),
                $"{card.Id} was asked to remove slot {slotSeqIdx} but it wasn't present — SequenceIndices: [{string.Join(",", mod.SequenceIndices)}]");
            if (mod.SequenceIndices.Count == 0)
            {
                CardModifier.DirectModifiers(card).Remove(mod);
                if (card.TryGetModifier<UnplayableModifier>(out var u))
                    CardModifier.DirectModifiers(card).Remove(u);
            }
        }
        else
        {
            Invariants.Check(false, nameof(PlannedModifier) + "." + nameof(RemoveSlot),
                $"{card.Id} was asked to remove slot {slotSeqIdx} but has no PlannedModifier at all");
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
