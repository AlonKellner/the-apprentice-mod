using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class MasterFormPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    // Generic (character-agnostic) "X Form": any Attack or Skill the owner plays that doesn't already
    // have Replay gains it, so it replays from then on. Idempotent — once BaseReplayCount is non-zero
    // it's never re-granted, and cards that already have Replay are left alone. A plain per-play power
    // hook, so (unlike the old Unplayable-event version) there's no static subscription to tear down.
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner?.Creature != Owner) return Task.CompletedTask;
        if (card.Type != CardType.Attack && card.Type != CardType.Skill) return Task.CompletedTask;
        if (card.BaseReplayCount == 0) card.BaseReplayCount = Amount;
        return Task.CompletedTask;
    }
}
