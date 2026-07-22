using MegaCrit.Sts2.Core.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Timeline;

// The Understudy's 7-chapter Timeline story ("The Final Lesson"): the Architect's dreamless quest to
// design his own ending, the perfect first child lost to the blight, the stolen dreaming boy, and the
// Final Lesson. These are real EpochModel/StoryModel types — the game has no mod API for them, so
// EpochRegistrar injects them into EpochModel's private static registries at init, and patches make
// them reachable from Neow and obtainable on the same progress conditions base characters use.
//
// IDs are numbered by narrative chronology (1..7). Timeline PLACEMENT (Era/EraPosition) follows that
// chronology along the game's era axis; every (Era, EraPosition) cell below was chosen from an audit
// of base epochs to avoid slot collisions. REVEAL order is deliberately different (driven by which
// condition each unlocks on) to land the twists — see UnderstudyEpochRevealPatch.
//
// Pure lore: QueueUnlocks does nothing (BaseLib already unlocks every Understudy card). Title,
// Description, UnlockInfo, and UnlockText all resolve from the mod's epochs.json by convention.
public abstract class UnderstudyEpoch : EpochModel
{
    // Slugify("TheUnderstudy") == "THE_UNDERSTUDY" (the StoryModel.Id we register); the loc story title
    // key is STORY_THEUNDERSTUDY (StoryTitle uses ToUpperInvariant, no underscore).
    public override string? StoryId => "TheUnderstudy";

    // Pure lore — nothing to unlock (cards are already available via BaseLib).
    public override void QueueUnlocks() { }
}

// 1 — Dreamless (Prehistoria). Reveals on: Beat Act 3.
public sealed class Understudy1Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY1_EPOCH";
    public override EpochEra Era => EpochEra.Prehistoria0;
    public override int EraPosition => 1;
}

// 2 — The Ending He Designed (Prehistoria). Reveals on: Beat Act 1.
public sealed class Understudy2Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY2_EPOCH";
    public override EpochEra Era => EpochEra.Prehistoria1;
    public override int EraPosition => 1;
}

// 3 — A Perfect Mirror (Prehistoria). Reveals on: Finish a run (the false promise, revealed first).
public sealed class Understudy3Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY3_EPOCH";
    public override EpochEra Era => EpochEra.Prehistoria2;
    public override int EraPosition => 2;
}

// 4 — Consumed (Blight). Reveals on: Complete Ascension 1 (the gut-punch, revealed last).
public sealed class Understudy4Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY4_EPOCH";
    public override EpochEra Era => EpochEra.Blight0;
    public override int EraPosition => 1;
}

// 5 — The Boy in the City (Flourish). Reveals on: Defeat 15 Bosses.
public sealed class Understudy5Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY5_EPOCH";
    public override EpochEra Era => EpochEra.Flourish0;
    public override int EraPosition => 3;
}

// 6 — Nothing Like Him (Flourish). Reveals on: Defeat 15 Elites.
public sealed class Understudy6Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY6_EPOCH";
    public override EpochEra Era => EpochEra.Flourish2;
    public override int EraPosition => 2;
}

// 7 — The Final Lesson (Invitation — the boy brought to the Spire). Reveals on: Beat Act 2.
public sealed class Understudy7Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY7_EPOCH";
    public override EpochEra Era => EpochEra.Invitation0;
    public override int EraPosition => 4;
}
