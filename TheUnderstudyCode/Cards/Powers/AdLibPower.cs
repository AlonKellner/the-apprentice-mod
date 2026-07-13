using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class AdLibPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Ad Lib",
        "At the end of your turn, Invert 1 random [gold]invertible[/gold] debuff you currently have.",
        "At the end of your turn, Invert {Amount} random [gold]invertible[/gold] debuff you currently have.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;

        var present = EmotionalExpression.GetPresentInvertibleDebuffs(Owner);
        if (present.Count == 0) return;

        var chosen = Owner.Player.RunState.Rng.CombatCardSelection.NextItem(present);
        await EmotionalExpression.InvertDebuff(context, Owner, chosen, Amount);
    }
}
