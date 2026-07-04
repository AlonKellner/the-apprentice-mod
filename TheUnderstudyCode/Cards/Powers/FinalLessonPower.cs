using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "Is this hurting you? Stop getting hurt. It will all be over soon. To the count of 3." Two effects
// bundled in one power, matching TheFirstLessonPower's own precedent of bundling related effects:
// (1) permanent, undecaying zero-HP-loss for the rest of combat, and (2) a 3-turn death countdown
// that counts the play turn as turn 1, using Amount itself as the visible "turns remaining" display.
public class FinalLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "The Final Lesson",
        "You cannot lose HP. At the end of your turn, if this is your 3rd turn since gaining this, you die.",
        "You cannot lose HP. {Amount} turn(s) remaining before you die.");

    // Late, mirroring BufferPower's own precedent: "other effects may reduce damage taken to 0 too,
    // and it's more player-friendly for them to trigger first." Functionally inert here (the result
    // is always exactly 0m regardless), but kept for consistency with that established idiom.
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return amount;
        return 0m;
    }

    // Pure countdown arithmetic: given the current Amount (turns remaining), what's the next Amount
    // and should the owner die this decrement. No engine dependency, directly unit-testable.
    public static (int newAmount, bool shouldDie) ComputeCountdown(int amount)
    {
        int next = amount - 1;
        return (next < 0 ? 0 : next, next <= 0);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner == null) return;
        var owner = Owner;
        var (_, shouldDie) = ComputeCountdown(Amount);
        Invariants.Check(Amount > 0, nameof(FinalLessonPower) + "." + nameof(BeforeSideTurnEnd),
            "about to decrement a Counter power that is already at 0 or below");
        await PowerCmd.Decrement(this);
        if (shouldDie) await CreatureCmd.Kill(owner, force: true);
    }
}
