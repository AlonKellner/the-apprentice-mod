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

// Turn-scoped marker applied by the Encore card. While present on a creature, using Vigor doesn't spend
// it: EncoreVigorRetentionPatch suppresses VigorPower's after-attack consumption (so Vigor keeps
// applying to every attack this turn), and DeceptiveCadence checks IsActive before spending Vigor for
// its Block. Removes itself at the end of the player's own turn, so it only lasts "this turn" — next
// turn Vigor is consumed normally again.
public class EncorePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Encore",
        "When you use [gold]Vigor[/gold] this turn, you keep it.",
        "When you use [gold]Vigor[/gold] this turn, you keep it.");

    // Checked from the sealed-VigorPower Harmony patch (mirrors MuscleMemoryPower/HeldNotePower.IsActive).
    public static bool IsActive(Creature? creature) => creature?.GetPower<EncorePower>() != null;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            await PowerCmd.Remove(this);
    }
}
