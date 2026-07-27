using System;
using System.Collections.Generic;
using System.Linq;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// The Book of Endings' study bank — the pure arithmetic behind "Study at Rest Sites to reveal an
// alternative Boss, up to 2 per act", factored out of the relic so it is unit-testable without the game
// (the injection it feeds needs a live ActMap and calls Log.*, neither of which the bare host can run).
//
// Studies do not belong to an act; they accumulate. Each act draws from the bank up to its OWN alt-boss
// capacity (AltBossPlan.AssignFlanks yields 0, 1 or 2 depending on the act's boss pool), and leaving the
// act consumes only what it actually absorbed — so a study the act had no room for survives into the
// next one and nothing is ever wasted.
//
// Two independent mechanisms sit on top of this. CanStudy disables the rest-site button once the act is
// full, which is what normally stops over-studying; the carry-over is the safety net for the co-op race
// where two players both select Study before the map redraws, which we deliberately do not try to
// serialise. Neither replaces the other.
//
// The bank is the one quantity here that cannot be re-derived from the run seed (unlike everything in
// AltBossPlan), so it lives in relic [SavedProperty] state — see BookOfEndings.
public static class AltBossReveal
{
    // Unspent studies. Clamped at zero: Consumed should never outrun Studies, but if it somehow did, a
    // negative bank would quietly mean "reveal nothing, forever" instead of failing visibly.
    public static int Bank(int studies, int consumed) => Math.Max(0, studies - consumed);

    // How many alternative bosses this act shows: the bank, capped by what the act has to offer.
    public static int RevealedInAct(int studies, int consumed, int capThisAct) =>
        Math.Min(Bank(studies, consumed), capThisAct);

    // Whether the rest-site Study option is usable. False once this act is full, so the button greys out
    // rather than letting the player spend a rest on nothing.
    public static bool CanStudy(int studies, int consumed, int capThisAct) =>
        Bank(studies, consumed) < capThisAct;

    // Leaving an act banks whatever it could not absorb; returns the new Consumed total.
    public static int Settle(int studies, int consumed, int capOfActBeingLeft) =>
        consumed + Math.Min(Bank(studies, consumed), capOfActBeingLeft);

    // The cap is party-wide, not per player — the map is shared, so the allowance is too. Each holder's
    // relic stores its own StudyCount; this is how they combine.
    public static int PartyStudies(IEnumerable<int> holderStudyCounts) => holderStudyCounts.Sum();
}
