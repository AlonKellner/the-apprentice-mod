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

// Buff mirror of TensionPower: at the end of the owner's turn the owner heals HP equal to its
// Untension, then it's removed. Same owner-gated single-hook shape as RegenPower, so it also works
// on enemies. Invertible buff side of the Tension pair; Swappable.
public class UntensionPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Untension",
        "At the end of its turn, this creature heals HP equal to its Untension, then it's removed. [gold]Invertible[/gold]. [gold]Swappable[/gold].",
        "At the end of its turn, this creature heals HP equal to its Untension, then it's removed. [gold]Invertible[/gold]. [gold]Swappable[/gold].");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.IsDead) return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
        await PowerCmd.Remove(this);
    }
}
