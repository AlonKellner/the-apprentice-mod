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

    // SPIKE (Phase 2, Spike A): inject ONE alternative boss node into each freshly generated map, at
    // the far-left of the boss row, wired as a child of the far-left pre-boss rest point. This proves
    // the fundamental unknowns — does an injected node render and can you travel to it — before the
    // full two-boss + per-node-encounter mechanic (Phase 3). ModifyGeneratedMap is the engine's own
    // hook for altering a generated map; running it here means it only fires when this relic is held.
    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
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
