using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnjadedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Hidden while empty — always attached from combat start (see
    // UnderstudyCard.AfterPlayerTurnStartLate) so its interception below can catch its own very
    // first incoming gain; only reveals itself once it actually holds stacks.
    protected override bool IsVisibleInternal => Amount > 0;

    // See JadedPower for why this uses {energyPrefix:energyIcons(3)} instead of
    // {Energy:energyIcons()} — GetDumbHoverTip (used by WithTip(typeof(UnjadedPower)) on other
    // cards) doesn't expose custom CanonicalVars, only Amount/singleStarIcon/energyPrefix.
    public override List<(string, string)> Localization => new PowerLoc(
        "Unjaded",
        "Gain {energyPrefix:energyIcons(3)} at the start of your next turn.",
        "Gain {energyPrefix:energyIcons(3)} at the start of your next turn.");

    private int _pendingJadedConsumption;
    private int _pendingSelfConsumption;

    // Bidirectional cancellation against JadedPower: this branch reduces an incoming Jaded gain
    // by my own current stock. The second branch reduces an incoming gain to MYSELF by however
    // much Jaded currently exists — this is what lets Invert's conversion (and each Fortissimo
    // repeat of it) re-check the live Jaded stock at the moment each individual gain lands,
    // rather than netting everything once up front.
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner || amount <= 0m) return false;

        if (canonicalPower is JadedPower)
        {
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, Amount);
            if (consumed <= 0) return false;
            _pendingJadedConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        if (ReferenceEquals(canonicalPower, this))
        {
            int curJaded = Owner.GetPowerAmount<JadedPower>();
            var (reduced, consumed) = EmotionalExpression.ComputeWeakCancellation((int)amount, curJaded);
            if (consumed <= 0) return false;
            _pendingSelfConsumption = consumed;
            modifiedAmount = reduced;
            return true;
        }
        return false;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        if (power is JadedPower && _pendingJadedConsumption > 0)
        {
            for (int i = 0; i < _pendingJadedConsumption; i++)
                await PowerCmd.Decrement(this);
            _pendingJadedConsumption = 0;
        }
        else if (ReferenceEquals(power, this) && _pendingSelfConsumption > 0)
        {
            var jaded = Owner.GetPower<JadedPower>();
            if (jaded == null)
            {
                Log.Error("UnjadedPower consumed Jaded stock via interception but " +
                          "GetPower<JadedPower> now finds none — the two are out of sync.");
            }
            else
            {
                for (int i = 0; i < _pendingSelfConsumption; i++)
                    await PowerCmd.Decrement(jaded);
            }
            _pendingSelfConsumption = 0;
        }
    }

    // See JadedPower for why this can't use ModifyEnergyGain — the natural per-turn refill
    // bypasses that hook entirely, so the bonus must be applied directly here instead.
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || player.PlayerCombatState == null || Amount <= 0) return;
        Flash();
        player.PlayerCombatState.GainEnergy(3m);
        if (!HeldNotePower.IsActive(Owner))
            await PowerCmd.Decrement(this);
    }
}
