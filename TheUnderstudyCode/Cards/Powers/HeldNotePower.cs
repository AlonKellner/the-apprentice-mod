using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class HeldNotePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    // Single, not Counter: Held Note is a flag (IsActive only checks presence), so it must not show a
    // stack number — an upgraded copy previously read "2" on its badge for no reason.
    public override PowerStackType StackType => PowerStackType.Single;


    // Two suppression paths cover every turn-based buff/debuff while Held Note is on the owner:
    //   1. Base-game duration powers (Weak, Vulnerable, Frail, Blur, Intangible, Temporary
    //      Str/Dex/Focus, Dampen, Shrink, Constrict, Poison-count, ...) all decrement through the
    //      single shared gate PowerCmd.TickDownDuration — HeldNoteTickDownPatch skips it generically.
    //   2. The mod's own invertible powers (Shaken/Jaded/Limited + their Un- buffs, Unweak,
    //      Unvulnerable, Tension, ...) self-decrement in their own turn hook and each check IsActive
    //      directly before decrementing, so they don't depend on the base-game gate.
    public static bool IsActive(Creature? creature) => creature?.GetPower<HeldNotePower>() != null;
}
