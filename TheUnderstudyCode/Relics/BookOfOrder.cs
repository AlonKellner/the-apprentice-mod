using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;
using BaseLib.Utils;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Unlocked form of the Architect's book, transformed from the Chaotic Book after three Studies.
// Its power — "the ending is ever changing... if studied, can be controlled" — is the Alternative
// Bosses map mechanic: two extra boss nodes flank the default, letting you route to the ending you
// choose. The mere presence of this relic on a player is the activation flag the map injection reads.
//
// Phase 1 is the shell: the map mechanic is wired in Phase 3 (see the plan). Event-obtained (no
// [Pool] to satisfy the analyzer); TheUnderstudyRelicPool excludes it as event-only, so it only ever
// exists as a transform target of ChaoticBook, never a reward.
[Pool(typeof(TheUnderstudyRelicPool))]
public class BookOfOrder : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
}
