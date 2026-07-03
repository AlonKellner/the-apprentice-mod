using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnshakenPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Hidden while empty — always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unshaken",
        "At the start of your turn, remove [gold]Unplayable[/gold] from all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.",
        "At the start of your turn, remove [gold]Unplayable[/gold] from all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.");

    private int _pendingShakenConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against ShakenPower: this branch reduces an incoming Shaken gain
    // by my own current stock. The second branch reduces an incoming gain to MYSELF by however
    // much Shaken currently exists — this is what lets Invert's conversion (and each Fortissimo
    // repeat of it) re-check the live Shaken stock at the moment each individual gain lands,
    // rather than netting everything once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is ShakenPower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingShakenConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curShaken = Owner.GetPowerAmount<ShakenPower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curShaken);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is ShakenPower && _pendingShakenConsumption > 0)
        {
            for (int i = 0; i < _pendingShakenConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingShakenConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var shaken = Owner.GetPower<ShakenPower>();
            if (shaken == null)
            {
                Log.Error("UnshakenPower consumed Shaken stock via interception but " +
                          "GetPower<ShakenPower> now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(shaken);
            }
            _pendingSelfConsumption = 0;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList().Where(c => c.Type == CardType.Attack || c.Type == CardType.Skill))
        {
            if (card.TryGetModifier<UnplayableModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);
        }
        if (Amount > 0 && !HeldNotePower.IsActive(Owner))
            await PowerCmd.Decrement(this);
    }
}
