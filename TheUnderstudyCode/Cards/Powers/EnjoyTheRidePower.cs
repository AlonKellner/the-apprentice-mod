using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Enjoy the Ride's deferred Invert: at the end of this turn, Invert Amount of each invertible
// debuff, then remove itself (one-shot, like CenterStagePower — not a recurring duration effect).
// Amount is the Invert value (2).
public class EnjoyTheRidePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;
        await EmotionalExpression.InvertEach(context, Owner, (int)Amount);
        // One-shot: gone after this single end-of-turn inversion (InvertEach already mutates powers
        // in this same hook, so removing here is the same established pattern as CenterStagePower).
        await PowerCmd.Remove(this);
    }
}
