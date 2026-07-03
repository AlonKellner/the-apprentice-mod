using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class TakeNotesPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Take Notes",
        "Whenever a debuff of yours clears, gain {Amount} Vigor.",
        "Whenever a debuff of yours clears, gain {Amount} Vigor.");

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        EmotionalExpression.DebuffCleared += OnDebuffCleared;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        EmotionalExpression.DebuffCleared -= OnDebuffCleared;
        return Task.CompletedTask;
    }

    private async Task OnDebuffCleared(PlayerChoiceContext ctx, Creature creature)
    {
        if (creature != Owner) return;
        await PowerCmd.Apply<VigorPower>(ctx, Owner, Amount, Owner, null, false);
    }
}
