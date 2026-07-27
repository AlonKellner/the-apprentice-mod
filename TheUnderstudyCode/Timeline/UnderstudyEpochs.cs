using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Events;
using TheUnderstudy.TheUnderstudyCode.Potions;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.Timeline;

// The Understudy's 7-chapter Timeline story ("The Final Lesson"): the Architect's dreamless quest to
// design his own ending, the perfect first child lost to the blight, the stolen dreaming boy, and the
// Final Lesson. These are real EpochModel/StoryModel types — the game has no mod API for them, so
// EpochRegistrar injects them into EpochModel's private static registries at init, and patches make
// them reachable from Neow and obtainable on the same progress conditions base characters use.
//
// IDs are numbered by narrative chronology (1..7). Timeline PLACEMENT (Era/EraPosition) weaves each
// chapter in next to the base epoch it belongs beside (columns left-to-right by ascending EpochEra;
// within a column EraPosition 0 = bottom, higher = top; every cell was checked free against a full
// base-epoch audit). REVEAL order is deliberately different (which condition unlocks each) to land the
// twists — see UnderstudyEpochRevealPatch.
//
// Each epoch UNLOCKS what its chapter is ABOUT, gated by TheUnderstudyCardPool/RelicPool/PotionPool's
// epoch filters (and, for the event, GoldenBedroom.IsAllowed) until the epoch is revealed. Card, relic
// and potion epochs unlock exactly THREE — the game's CreateXUnlockText helpers loop [0..2] and throw on
// any other count — and use the game's own formatter, auto-colored by rarity. "Writing an End" instead
// unlocks a single EVENT, following the base game's Event1Epoch/Event2Epoch/Event3Epoch shape: its own
// .unlockText loc key and QueueMiscUnlock, not the 3-item helpers.
//
// RULE: only Uncommon/Rare content is ever gated. Commons are a deck's connective tissue and must be
// available from the first run — withholding them makes early runs feel thin for no thematic gain.
// Enforced by EpochUnlockTests.
public abstract class UnderstudyEpoch : EpochModel
{
    // Slugify("TheUnderstudy") == "THE_UNDERSTUDY" (the StoryModel.Id we register); the loc story title
    // key is STORY_THEUNDERSTUDY (StoryTitle uses ToUpperInvariant, no underscore).
    public override string? StoryId => "TheUnderstudy";
}

// 1 — Dreamless (Seeds0, after "The Architect"). Reveals on: Beat Act 1. Unlocks 3 relics — the props
// of the trade the boy will be pressed into. (SafetyNet is deliberately NOT here: it is Common.)
public sealed class Understudy1Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY1_EPOCH";
    public override EpochEra Era => EpochEra.Seeds0;
    public override int EraPosition => 2;

    public static List<RelicModel> Relics => new()
        { ModelDb.Relic<Greasepaint>(), ModelDb.Relic<Rosin>(), ModelDb.Relic<Lozenge>() };

    public override string UnlockText => CreateRelicUnlockText(Relics);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueRelicUnlock(Relics);
}

// 2 — Writing an End (Seeds1, below "Ancients"). Reveals on: Beat Act 2. Unlocks the Golden Bedroom
// event — the chapter is the Architect resolving to write his ending in a book, and that event is where
// you find the book (it grants the Book of Endings).
//
// Event epochs are their own shape in the base game (Event1Epoch/Event2Epoch/Event3Epoch): a one-entry
// Events list, a hand-written .unlockText loc key with an {Event} placeholder, and QueueMiscUnlock —
// NOT CreateCardUnlockText, which requires exactly 3 entries. The actual gate lives in
// GoldenBedroom.IsAllowed.
public sealed class Understudy2Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY2_EPOCH";
    public override EpochEra Era => EpochEra.Seeds1;
    public override int EraPosition => 0;

    public static List<EventModel> Events => new() { ModelDb.Event<GoldenBedroom>() };

    public override string UnlockText
    {
        get
        {
            var text = new LocString("epochs", Id + ".unlockText");
            text.Add("Event", Events[0].Title);
            return text.GetFormattedText();
        }
    }

    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueMiscUnlock(UnlockText);
}

// 3 — A Perfect Mirror (Seeds2, after ep2). Reveals on: Finish a run. All three unlocks are repetition —
// Master Form's Replay echo, a Motif that re-Plans itself, and the Magnum Opus that the perfect child
// was meant to be.
public sealed class Understudy3Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY3_EPOCH";
    public override EpochEra Era => EpochEra.Seeds2;
    public override int EraPosition => 4;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<MasterForm>(), ModelDb.Card<Motif>(), ModelDb.Card<MagnumOpus>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 4 — Consumed. After "Consequences" (Relic4 @ Blight0 pos0, the bottom/last of its column) = top of the
// next column, Blight1 (pos4) — it's the aftermath of Consequences. Reveals on: Ascension 1+. Unlocks 3
// potions: the blight consumed the child, and potions are the things you consume.
public sealed class Understudy4Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY4_EPOCH";
    public override EpochEra Era => EpochEra.Blight1;
    public override int EraPosition => 4;

    public static List<PotionModel> Potions => new()
        { ModelDb.Potion<PlannedPotion>(), ModelDb.Potion<TunedPotion>(), ModelDb.Potion<SwapPotion>() };

    public override string UnlockText => CreatePotionUnlockText(Potions);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueuePotionUnlock(Potions);
}

// 5 — Taken (Flourish). Reveals on: Defeat 15 Bosses. The boy is carried off and started over: Clean
// Slate wipes the board, Held Note freezes the turn clock like a life suspended, and the teaching begins
// with The First Lesson.
public sealed class Understudy5Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY5_EPOCH";
    public override EpochEra Era => EpochEra.Flourish2;
    public override int EraPosition => 2;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<TheFirstLesson>(), ModelDb.Card<HeldNote>(), ModelDb.Card<CleanSlate>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 6 — Nothing Like Him (before "Underdocks"). Reveals on: Defeat 15 Elites. The epoch's whole tension in
// three cards: The Second Lesson's Orders against the music the boy keeps wandering off into.
public sealed class Understudy6Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY6_EPOCH";
    public override EpochEra Era => EpochEra.Invitation0;
    public override int EraPosition => 4;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<TheSecondLesson>(), ModelDb.Card<Reverb>(), ModelDb.Card<SonicBoom>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 7 — Every Story has a Lesson (the confrontation). Reveals on: Beat Act 3. The Final Test: Fate Knocking
// pays the whole combat back in one blow (its 3 strikes echoing the Lesson's 3-turn countdown), and One
// Take gives you no second chances. My Own Lesson is deliberately NOT here — the boy's own conclusion is
// not something the Architect's story grants, so it is always available.
public sealed class Understudy7Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY7_EPOCH";
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 3;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<TheFinalLesson>(), ModelDb.Card<FateKnocking>(), ModelDb.Card<OneTake>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}
