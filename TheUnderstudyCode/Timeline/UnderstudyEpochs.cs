using MegaCrit.Sts2.Core.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Timeline;

// The Understudy's 7-chapter Timeline story ("The Final Lesson"): the Architect's dreamless quest to
// design his own ending, the perfect first child lost to the blight, the stolen dreaming boy, and the
// Final Lesson. These are real EpochModel/StoryModel types — the game has no mod API for them, so
// EpochRegistrar injects them into EpochModel's private static registries at init, and patches make
// them reachable from Neow and obtainable on the same progress conditions base characters use.
//
// IDs are numbered by narrative chronology (1..7). Timeline PLACEMENT (Era/EraPosition) weaves each
// chapter in next to the base epoch it belongs beside (columns run left-to-right by ascending EpochEra;
// within a column EraPosition 0 = bottom, higher = up = "after"). Anchors: "The Architect" =
// Colorless2Epoch @ Prehistoria2 pos0; "Ancients" = Relic2Epoch @ Seeds1 pos1; "Underdocks" @
// Invitation1 pos4. Every cell below was checked free against a full base-epoch audit. REVEAL order is
// deliberately different (driven by which condition each unlocks on) to land the twists — see
// UnderstudyEpochRevealPatch.
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

// 1 — Dreamless. Immediately after "The Architect" (Prehistoria2 pos0); nearest free cell above the
// Silent6/"Nemesis" slot at pos1. Reveals on: Beat Act 3.
public sealed class Understudy1Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY1_EPOCH";
    public override EpochEra Era => EpochEra.Prehistoria2;
    public override int EraPosition => 2;
}

// 2 — The Ending He Designed. Immediately after "Ancients" (Relic2 @ Seeds1 pos1). Reveals on: Beat Act 1.
public sealed class Understudy2Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY2_EPOCH";
    public override EpochEra Era => EpochEra.Seeds1;
    public override int EraPosition => 2;
}

// 3 — A Perfect Mirror. Immediately after "The Ending He Designed" (Seeds1 pos2). Reveals on: Finish a run.
public sealed class Understudy3Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY3_EPOCH";
    public override EpochEra Era => EpochEra.Seeds1;
    public override int EraPosition => 3;
}

// 4 — Consumed (Blight — the child unmade). Reveals on: Complete Ascension 1 (the gut-punch, last).
public sealed class Understudy4Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY4_EPOCH";
    public override EpochEra Era => EpochEra.Blight0;
    public override int EraPosition => 1;
}

// 5 — The Boy in the City. Just before "Underdocks" (Invitation1 pos4, full) — the column before it.
// Reveals on: Defeat 15 Bosses.
public sealed class Understudy5Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY5_EPOCH";
    public override EpochEra Era => EpochEra.Invitation0;
    public override int EraPosition => 4;
}

// 6 — Nothing Like Him (Flourish — the molding years). Reveals on: Defeat 15 Elites.
public sealed class Understudy6Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY6_EPOCH";
    public override EpochEra Era => EpochEra.Flourish2;
    public override int EraPosition => 2;
}

// 7 — The Final Lesson (Invitation — the boy at the Spire). Reveals on: Beat Act 2.
public sealed class Understudy7Epoch : UnderstudyEpoch
{
    public override string Id => "THEUNDERSTUDY7_EPOCH";
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 3;
}
