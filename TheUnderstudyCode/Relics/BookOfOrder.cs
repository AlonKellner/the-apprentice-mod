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
// Bosses map mechanic: two extra boss nodes flank the default, letting you route to the ending you
// choose. The mere presence of this relic on a player is the activation flag the map injection reads.
//
// Event-obtained (no [Pool] to satisfy the analyzer); TheUnderstudyRelicPool excludes it as event-only,
// so it only ever exists as a transform target of ChaoticBook, never a reward.
[Pool(typeof(TheUnderstudyRelicPool))]
public class BookOfOrder : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    // SPIKE (Phase 2): inject ONE alternative boss node at the far-left of the boss row, wired as a
    // child of the far-left pre-boss rest point. Injection lives in ModifyGeneratedMapLate, not
    // ModifyGeneratedMap, because that hook fires on BOTH paths: fresh generation (Hook.ModifyGeneratedMap
    // ends by calling ModifyGeneratedMapLate) AND load (GenerateMap's saved-map branch calls only
    // ModifyGeneratedMapLate). So the same code re-injects deterministically on load — the base save
    // format has no slot for alt bosses, so they are re-created rather than serialized. Idempotent via
    // the store check, so it can't double-inject.
    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        if (AltBossStore.For(map).Count > 0) return map; // already injected on this map instance

        var preBossRow = map.GetPointsInRow(map.GetRowCount() - 1).ToList();
        if (preBossRow.Count == 0)
        {
            Log.Warn($"[BookOfOrder] act {actIndex}: no pre-boss row, cannot inject alt boss");
            return map;
        }

        var farLeft = preBossRow.OrderBy(p => p.coord.col).First();
        int bossRow = map.BossMapPoint.coord.row;
        var altBoss = new MapPoint(0, bossRow) { PointType = MapPointType.Boss };
        farLeft.AddChildPoint(altBoss);
        AltBossStore.Register(map, altBoss);

        Log.Info($"[BookOfOrder] act {actIndex}: injected alt boss at ({altBoss.coord.col},{altBoss.coord.row}) " +
                 $"wired from far-left pre-boss rest ({farLeft.coord.col},{farLeft.coord.row}); " +
                 $"boss at ({map.BossMapPoint.coord.col},{map.BossMapPoint.coord.row}), " +
                 $"GetAllMapPoints now sees {map.GetAllMapPoints().Count()} points");
        return map;
    }
}
