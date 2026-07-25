using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Map;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Unlocked form of the Architect's book, transformed from the Chaotic Book after three Studies.
// Its power — "the ending is ever changing... if studied, can be controlled" — is the Alternative
// Bosses map mechanic: two extra boss nodes flank the default, one reachable from the far-left pre-boss
// rest and one from the far-right, each leading to a *different* act boss. The default boss keeps its
// own untouched node, so all three bosses of the act are reachable and choosing your ending becomes
// routing. The mere presence of this relic on a player is the activation flag the injection reads.
//
// Event-obtained; TheUnderstudyRelicPool excludes it as event-only, so it only ever exists as a
// transform target of ChaoticBook, never a random reward.
[Pool(typeof(TheUnderstudyRelicPool))]
public class BookOfOrder : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    // Inject the flank boss nodes. This runs in ModifyGeneratedMapLate (not ModifyGeneratedMap) because
    // that hook fires on BOTH paths: fresh generation (Hook.ModifyGeneratedMap ends by calling it) AND
    // load (GenerateMap's saved-map branch calls only it). The base save format has no slot for alt
    // bosses, so instead of serializing them they are re-derived here from AltBossPlan — a pure function
    // of the run seed + act index — so the same two nodes, with the same left/right encounter
    // assignment, reappear identically on load and on every co-op client. Idempotent via the store
    // check, so it can't double-inject.
    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        if (AltBossStore.For(map).Count > 0) return map; // already injected on this map instance

        var act = runState.Acts[actIndex];
        var allBossIds = act.AllBossEncounters.Select(e => e.Id.ToString()).ToList();
        var defaultId = act.BossEncounter.Id.ToString();
        ulong seed = AltBossPlan.SeedFor(runState.Rng.Seed, actIndex);
        var flanks = AltBossPlan.AssignFlanks(allBossIds, defaultId, seed);
        if (flanks.Count == 0)
        {
            Log.Info($"[BookOfOrder] act {actIndex}: only one boss in pool, no alt bosses to inject");
            return map;
        }

        var preBossRow = map.GetPointsInRow(map.GetRowCount() - 1).ToList();
        if (preBossRow.Count == 0)
        {
            Log.Warn($"[BookOfOrder] act {actIndex}: no pre-boss row, cannot inject alt bosses");
            return map;
        }

        int bossRow = map.BossMapPoint.coord.row;
        int cols = map.GetColumnCount();
        int defaultCol = map.BossMapPoint.coord.col;
        var farLeftRest = preBossRow.OrderBy(p => p.coord.col).First();
        var farRightRest = preBossRow.OrderByDescending(p => p.coord.col).First();

        var injected = new List<string>();
        var flankNodes = new List<(FlankSide side, string enc, MapPoint node)>();
        foreach (var (side, encounterId) in flanks)
        {
            // Left node hugs col 0, right node hugs the last column, so they render on either side of the
            // centre default boss; each wires as a child of the pre-boss rest on its own side.
            int col = side == FlankSide.Left ? 0 : cols - 1;
            var rest = side == FlankSide.Left ? farLeftRest : farRightRest;

            if (col == defaultCol)
            {
                Invariants.Check(false, "BookOfOrder",
                    $"alt boss col {col} collides with default boss col on a {cols}-wide map; skipping {side}");
                continue;
            }

            var altBoss = new MapPoint(col, bossRow) { PointType = MapPointType.Boss };
            rest.AddChildPoint(altBoss);
            AltBossStore.Register(map, new AltBossNode(altBoss, side, encounterId, rest.coord, IsSecond: false));
            flankNodes.Add((side, encounterId, altBoss));
            injected.Add($"{side}=({col},{bossRow})->{encounterId} from rest ({rest.coord.col},{rest.coord.row})");
        }

        // Double boss (Ascension 10 final act): chain a second boss above each flank. The seconds are a
        // derangement of the reachable first bosses (default + both flanks), so every boss chains into a
        // *different* reachable boss — no self-pairing. Override the default boss's second to the same
        // derangement so all three endings cycle cleanly.
        bool doubleBoss = act.HasSecondBoss;
        if (doubleBoss)
        {
            var firsts = new List<string> { defaultId };
            firsts.AddRange(flankNodes.Select(f => f.enc));
            var derange = AltBossPlan.Derange(firsts, seed);

            if (act.HasSecondBoss && derange.TryGetValue(defaultId, out var dSecondId))
            {
                var dSecond = act.AllBossEncounters.FirstOrDefault(e => e.Id.ToString() == dSecondId);
                if (dSecond != null) act.SetSecondBossEncounter(dSecond);
            }

            foreach (var f in flankNodes)
            {
                if (!derange.TryGetValue(f.enc, out var secondEnc)) continue;
                var second = new MapPoint(f.node.coord.col, bossRow + 1) { PointType = MapPointType.Boss };
                f.node.AddChildPoint(second);
                AltBossStore.Register(map, new AltBossNode(second, f.side, secondEnc, f.node.coord, IsSecond: true));
                injected.Add($"{f.side}#2=({second.coord.col},{second.coord.row})->{secondEnc} chained from {f.enc}");
            }

            Invariants.Check(flankNodes.All(f => !derange.TryGetValue(f.enc, out var s) || s != f.enc),
                "BookOfOrder", "a flank second boss self-pairs (derangement failed)");
        }

        Log.Info($"[BookOfOrder] act {actIndex}: seed {runState.Rng.Seed}->{seed}, default={defaultId} " +
                 $"at col {defaultCol}; doubleBoss={doubleBoss}; injected [{string.Join(", ", injected)}]; " +
                 $"GetAllMapPoints now sees {map.GetAllMapPoints().Count()} points");
        return map;
    }
}
