using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// The first time your hand holds an Unplayable card in a given turn, draw Amount cards. Checked at
// turn start (hand may already hold one) and after each card play (a play may have just jammed a
// card). Amount = cards drawn.
// NOTE: turn/hook-timing behavior — verify in-game (no combat test harness).
public class ResourcefulPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Resourceful",
        "The first time you have an [gold]Unplayable[/gold] card in hand each turn, draw this many cards.",
        "The first time you have an [gold]Unplayable[/gold] card in hand each turn, draw {Amount} cards.");

    private bool _triggeredThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        _triggeredThisTurn = false;
        await MaybeTrigger(context, player);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Owner.Player != null)
            await MaybeTrigger(context, Owner.Player);
    }

    private async Task MaybeTrigger(PlayerChoiceContext context, Player player)
    {
        if (_triggeredThisTurn) return;
        if (!PileType.Hand.GetPile(player).Cards.Any(c => c.IsUnplayable())) return;
        _triggeredThisTurn = true;
        await CardPileCmd.Draw(context, (int)Amount, player);
    }
}
