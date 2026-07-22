using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using TheUnderstudy.TheUnderstudyCode.Cards;
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
// Each epoch genuinely UNLOCKS 3 mechanics (cards/relics/potions), gated by TheUnderstudyCardPool/
// RelicPool/PotionPool's epoch filters until the epoch is revealed. UnlockText/QueueUnlocks use the game's
// own formatter (auto-colored by rarity).
public abstract class UnderstudyEpoch : EpochModel
{
    // Slugify("TheUnderstudy") == "THE_UNDERSTUDY" (the StoryModel.Id we register); the loc story title
    // key is STORY_THEUNDERSTUDY (StoryTitle uses ToUpperInvariant, no underscore).
    public override string? StoryId => "TheUnderstudy";
}

// 1 — Dreamless (Seeds0, after "The Architect"). Reveals on: Beat Act 1.
public sealed class Understudy1Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY1_EPOCH";
    public override EpochEra Era => EpochEra.Seeds0;
    public override int EraPosition => 2;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<Foreshadow>(), ModelDb.Card<Orchestration>(), ModelDb.Card<MagnumOpus>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 2 — Designing an End (Seeds1, below "Ancients"). Reveals on: Beat Act 2.
public sealed class Understudy2Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY2_EPOCH";
    public override EpochEra Era => EpochEra.Seeds1;
    public override int EraPosition => 0;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<TheFirstLesson>(), ModelDb.Card<TheSecondLesson>(), ModelDb.Card<HeldNote>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 3 — A Perfect Mirror (Seeds2, after ep2). Reveals on: Finish a run.
public sealed class Understudy3Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY3_EPOCH";
    public override EpochEra Era => EpochEra.Seeds2;
    public override int EraPosition => 4;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<MasterForm>(), ModelDb.Card<Showstopper>(), ModelDb.Card<Motif>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 4 — Consumed (Blight — the child unmade). Reveals on: Complete Ascension 1. Unlocks 3 relics.
public sealed class Understudy4Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY4_EPOCH";
    public override EpochEra Era => EpochEra.Blight0;
    public override int EraPosition => 1;

    public static List<RelicModel> Relics => new()
        { ModelDb.Relic<Greasepaint>(), ModelDb.Relic<Rosin>(), ModelDb.Relic<SafetyNet>() };

    public override string UnlockText => CreateRelicUnlockText(Relics);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueRelicUnlock(Relics);
}

// 5 — Taken (before "Underdocks"). Reveals on: Defeat 15 Bosses. Unlocks 3 potions.
public sealed class Understudy5Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY5_EPOCH";
    public override EpochEra Era => EpochEra.Invitation0;
    public override int EraPosition => 4;

    public static List<PotionModel> Potions => new()
        { ModelDb.Potion<PlannedPotion>(), ModelDb.Potion<TunedPotion>(), ModelDb.Potion<SwapPotion>() };

    public override string UnlockText => CreatePotionUnlockText(Potions);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueuePotionUnlock(Potions);
}

// 6 — Nothing Like Him (the boy's music). Reveals on: Defeat 15 Elites.
public sealed class Understudy6Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY6_EPOCH";
    public override EpochEra Era => EpochEra.Flourish2;
    public override int EraPosition => 2;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<Melody>(), ModelDb.Card<Reverb>(), ModelDb.Card<SonicBoom>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}

// 7 — Every Story has a Lesson (the confrontation). Reveals on: Beat Act 3.
public sealed class Understudy7Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY7_EPOCH";
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 3;

    public static List<CardModel> Cards => new()
        { ModelDb.Card<TheFinalLesson>(), ModelDb.Card<MyOwnLesson>(), ModelDb.Card<OwnIt>() };

    public override string UnlockText => CreateCardUnlockText(Cards);
    public override void QueueUnlocks() => NTimelineScreen.Instance.QueueCardUnlock(Cards);
}
