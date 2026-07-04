using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnlimitedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unlimited",
        "At the start of your next turn, draw until your hand is full.",
        "At the start of your next turn, draw until your hand is full.");

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player) return count;
        // CardPileCmd.Draw already clamps its own draw loop to CardPile.MaxCardsInHand,
        // stopping early once the hand is full — requesting the cap as the draw count is
        // always enough to reach it (or drain the deck trying) without a manual hand-size read.
        return Math.Max(count, CardPile.MaxCardsInHand);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        if (!HeldNotePower.IsActive(Owner))
        {
            Invariants.Check(Amount > 0, nameof(UnlimitedPower) + "." + nameof(AfterSideTurnStart),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
