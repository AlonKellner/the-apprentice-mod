using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Debug-only relic: while any player holds it, the Book of Order chains a second boss above each flank
// on EVERY act, not just the Ascension-10 final act, so the double-boss cycle can be exercised without
// grinding a full Asc10 run to the last boss. Add it in the dev console with
// `relic add THEUNDERSTUDY-ALT_BOSS_DOUBLE_BOSS_DEBUG`. Carries [Pool] to satisfy the analyzer but is
// excluded from real reward pools (TheUnderstudyRelicPool.EventOnlyRelics), so it never drops.
[Pool(typeof(TheUnderstudyRelicPool))]
public class AltBossDoubleBossDebug : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public static bool IsActive(IRunState runState) =>
        runState.Players.Any(p => p.Relics.Any(r => r is AltBossDoubleBossDebug));
}
