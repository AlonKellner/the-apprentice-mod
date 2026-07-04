using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class MasterFormPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Master Form",
        "Whenever an attack or skill that doesn't have Replay becomes [gold]Unplayable[/gold], it gains Replay 1.",
        "Whenever an attack or skill that doesn't have Replay becomes [gold]Unplayable[/gold], it gains Replay 1.");

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied += OnUnplayableApplied;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
        return Task.CompletedTask;
    }

    private void OnUnplayableApplied(CardModel card)
    {
        if (card.Owner?.Creature != Owner) return;
        if (card.BaseReplayCount == 0) card.BaseReplayCount = 1;
    }
}
