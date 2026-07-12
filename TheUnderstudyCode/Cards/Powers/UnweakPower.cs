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

public class UnweakPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unweak",
        "Unweakened creatures deal [blue]25%[/blue] more damage with Attacks. Cancels out with Weak. [gold]Invertible[/gold].",
        "Unweakened creatures deal [blue]25%[/blue] more damage with Attacks. Cancels out with Weak. [gold]Invertible[/gold].");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 1.25m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && !HeldNotePower.IsActive(Owner))
            await PowerCmd.TickDownDuration(this);
    }
}
