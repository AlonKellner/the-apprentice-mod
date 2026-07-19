using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Duration buff applied by the Reverb card. While present on a creature, using Vigor doesn't spend it:
// ReverbVigorRetentionPatch suppresses VigorPower's after-attack consumption (so Vigor keeps applying to
// every attack), and DeceptiveCadence checks IsActive before spending Vigor for its Block. Amount is the
// number of your turns the retention lasts; it stacks (play twice -> retained 2 turns) and ticks down by
// 1 at the end of each of your turns, removing itself at 0. Counter (not Single) so the turn count shows.
public class ReverbPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Reverb",
        "When you use [gold]Vigor[/gold], you keep it.",
        "When you use [gold]Vigor[/gold], you keep it. Lasts [blue]{Amount}[/blue] more {Amount:plural:turn|turns}.");

    // Checked from the sealed-VigorPower Harmony patch (mirrors MuscleMemoryPower/HeldNotePower.IsActive).
    // Presence-only, so it's unaffected by the stacking Amount.
    public static bool IsActive(Creature? creature) => creature?.GetPower<ReverbPower>() != null;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            await PowerCmd.TickDownDuration(this);
    }
}
