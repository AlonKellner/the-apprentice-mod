using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class DressRehearsalPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Dress Rehearsal",
        "At the start of your next turn, [gold]Invert[/gold] this many of each invertible debuff.",
        "At the start of your next turn, [gold]Invert[/gold] [blue]{Amount}[/blue] of each invertible debuff.");

    // One-shot: fires exactly once on the very next turn start, then removes itself outright
    // rather than decaying — unlike the recurring Un-X powers (UnshakenPower et al.), this isn't
    // a duration effect, it's a single deferred payoff for DressRehearsal's up-front self-debuffs.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        await EmotionalExpression.InvertEach(context, Owner, (int)Amount);
        await PowerCmd.Remove(this);
    }
}
