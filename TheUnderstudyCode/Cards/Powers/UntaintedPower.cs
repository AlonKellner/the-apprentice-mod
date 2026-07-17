using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// The buff side of the Tainted pair. Mirror of the base-game TaintedPower: instead of the owner
// taking +Amount from powered Attacks, it takes -Amount (less). Owner-side-aware removal from the
// start (remove at the end of the opponent's turn, after the owner was attacked) — the same
// lifecycle TaintedSwappablePatch gives base Tainted, so both work on either side. Invert converts
// Tainted -> Untainted. Swappable.
public class UntaintedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Untainted",
        "This creature takes [blue]this much[/blue] less damage from [gold]Attacks[/gold] this turn. [gold]Invertible[/gold]. [gold]Swappable[/gold].",
        "This creature takes [blue]this much[/blue] less damage from [gold]Attacks[/gold] this turn. [gold]Invertible[/gold]. [gold]Swappable[/gold].");

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        return -Amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // Clear at the end of the opponent's turn (after the owner has been attacked), so it actually
        // reduces incoming Attack damage regardless of which side owns it.
        if (side != Owner.Side)
        {
            Flash();
            await PowerCmd.Remove(this);
        }
    }
}
