using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class HeldNotePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Held Note",
        "Invertible debuffs and buffs no longer decrease by 1 each turn.",
        "Invertible debuffs and buffs no longer decrease by 1 each turn.");

    // Checked directly by Shaken/Unshaken/Limited/Unlimited/Jaded/Unjaded/Unweak/Unvulnerable
    // before their own per-turn decrement, so suppression doesn't depend on hook execution order
    // between different Powers. WeakPower/VulnerablePower are sealed base-game classes we can't
    // add a matching guard to, so those two are instead suppressed via SkipNextDurationTick below.
    public static bool IsActive(Creature? creature) => creature?.GetPower<HeldNotePower>() != null;

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        // BeforeSideTurnEnd is guaranteed to run before AfterSideTurnEnd for the same side, which
        // is where base-game Weak/Vulnerable tick down (per SkipNextDurationTick's own doc comment).
        if (side == CombatSide.Enemy && Owner != null)
        {
            var weak = Owner.GetPower<WeakPower>();
            if (weak != null) weak.SkipNextDurationTick = true;
            var vulnerable = Owner.GetPower<VulnerablePower>();
            if (vulnerable != null) vulnerable.SkipNextDurationTick = true;
        }
        return Task.CompletedTask;
    }
}
