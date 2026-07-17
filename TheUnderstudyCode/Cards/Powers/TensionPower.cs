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
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Universal (owner-relative) debuff: at the end of the OWNER's turn the owner takes unblockable
// damage equal to its Tension, then it's fully removed. The participants.Contains(Owner) gate makes
// it fire exactly once at the end of whichever side the owner is on (the RegenPower idiom), so it
// works when Swap hands it to enemies. Invertible: pairs with UntensionPower (Tension = debuff side).
public class TensionPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Tension",
        "At the end of its turn, this creature takes damage equal to its Tension, then it's removed. [gold]Invertible[/gold]. [gold]Swappable[/gold].",
        "At the end of its turn, this creature takes damage equal to its Tension, then it's removed. [gold]Invertible[/gold]. [gold]Swappable[/gold].");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.IsDead) return;
        Flash();
        await CreatureCmd.Damage(context, Owner, Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await PowerCmd.Remove(this);
    }
}
