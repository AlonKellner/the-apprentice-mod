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
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnlimitedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player) return count;
        // This is the effect site, so it is also what licenses the tick-down below — see
        // UnderstudyPower's marker. A stack granted after this turn's draw (Punished applies at
        // AfterPlayerTurnStartLate, which is post-draw) never gets marked and so never decays for a
        // turn it did nothing on.
        MarkTookEffectThisTurn();
        // CardPileCmd.Draw already clamps its own draw loop to CardPile.MaxCardsInHand,
        // stopping early once the hand is full — requesting the cap as the draw count is
        // always enough to reach it (or drain the deck trying) without a manual hand-size read.
        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        Log.Info($"[LessonDiag] UnlimitedPower.ModifyHandDraw[turn {turn}]: Amount={Amount}, draw {count} -> {Math.Max(count, CardPile.MaxCardsInHand)} (draw to full)");
        return Math.Max(count, CardPile.MaxCardsInHand);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        bool heldNote = HeldNotePower.IsActive(Owner);
        bool tookEffect = ConsumeTookEffectThisTurn();
        Log.Info($"[LessonDiag] UnlimitedPower.AfterSideTurnStart[turn {turn}, side {side}]: Amount={Amount}, heldNote={heldNote}, tookEffect={tookEffect} -> {(heldNote ? "no decrement (Held Note)" : tookEffect ? "decrement -1" : "no decrement (did not take effect this turn)")}");
        if (!heldNote && tookEffect)
        {
            Invariants.Check(Amount > 0, nameof(UnlimitedPower) + "." + nameof(AfterSideTurnStart),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
