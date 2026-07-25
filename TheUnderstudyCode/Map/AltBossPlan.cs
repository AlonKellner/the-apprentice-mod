using System.Collections.Generic;
using System.Linq;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// Which side of the default boss an injected alternative boss sits on.
public enum FlankSide
{
    Left,
    Right,
}

// The pure, deterministic planning behind the Book of Order's Alternative Bosses mechanic, factored out
// of the map-injection code so it is unit-testable without the game (the injection itself constructs
// MapPoints, mutates the live ActMap, and calls Log.* — none of which the bare test host can run).
//
// Everything here is a pure function of a run seed + act index, so the same plan is produced on fresh
// generation, on load (re-injection), and on every co-op client — which is exactly what keeps the map
// in sync without serializing the alt bosses. See BookOfOrder / AltBossStore for the game-side wiring.
public static class AltBossPlan
{
    // A stable per-act seed derived from the run seed and the act index (splitmix64 finalizer, so
    // adjacent acts don't produce correlated low bits). Pure — no RNG stream is consumed, so injecting
    // the plan never advances shared run state or desyncs other seeded systems.
    public static ulong SeedFor(ulong runSeed, int actIndex)
    {
        ulong z = runSeed + 0x9E3779B97F4A7C15UL * (ulong)(actIndex + 1);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    // Assign the non-default bosses of an act to the left/right flank nodes. The default boss stays
    // reachable via its own (untouched) node, so this only places the *other* bosses:
    //   3-boss act (the base-game norm) -> both others, one per flank; all three bosses reachable.
    //   2-boss act                      -> the single other boss on one seeded flank.
    //   1-boss act                      -> no alt bosses (nothing to choose between).
    //   4+-boss act                     -> the first two others in stable order, one per flank.
    // The assignment is deterministic in the seed, so left/right is stable across reload and co-op.
    public static IReadOnlyList<(FlankSide side, string encounterId)> AssignFlanks(
        IReadOnlyList<string> allBossIds, string defaultBossId, ulong seed)
    {
        var others = allBossIds.Where(id => id != defaultBossId).Distinct().ToList();
        if (others.Count == 0)
            return System.Array.Empty<(FlankSide, string)>();

        if (others.Count == 1)
        {
            var side = (seed & 1UL) == 0 ? FlankSide.Left : FlankSide.Right;
            return new[] { (side, others[0]) };
        }

        // Two flanks: take the first two others in stable order and decide their sides by one seed bit.
        var a = others[0];
        var b = others[1];
        return (seed & 1UL) == 0
            ? new[] { (FlankSide.Left, a), (FlankSide.Right, b) }
            : new[] { (FlankSide.Left, b), (FlankSide.Right, a) };
    }

    // A derangement of the reachable first bosses onto second bosses for the Ascension-10 double-boss
    // final act: every first boss chains into a *different* second boss (no self-pairing). Implemented
    // as a cyclic shift by one over the stable-ordered, de-duplicated ids — a permutation with no fixed
    // point for any count >= 2. Deterministic in the seed (which only chooses the rotation direction).
    public static IReadOnlyDictionary<string, string> Derange(IReadOnlyList<string> ids, ulong seed)
    {
        var distinct = ids.Distinct().ToList();
        var map = new Dictionary<string, string>();
        int n = distinct.Count;
        if (n < 2)
            return map; // a single boss cannot chain into a different one; caller falls back.

        int step = (seed & 1UL) == 0 ? 1 : n - 1; // rotate forward or backward; neither has a fixed point
        for (int i = 0; i < n; i++)
            map[distinct[i]] = distinct[(i + step) % n];
        return map;
    }
}
