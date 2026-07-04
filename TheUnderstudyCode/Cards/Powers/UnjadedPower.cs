using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnjadedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // See JadedPower for why this uses {energyPrefix:energyIcons(3)} instead of
    // {Energy:energyIcons()} — GetDumbHoverTip (used by WithTip(typeof(UnjadedPower)) on other
    // cards) doesn't expose custom CanonicalVars, only Amount/singleStarIcon/energyPrefix.
    public override List<(string, string)> Localization => new PowerLoc(
        "Unjaded",
        "Gain {energyPrefix:energyIcons(3)} at the start of your next turn.",
        "Gain {energyPrefix:energyIcons(3)} at the start of your next turn.");

    // See JadedPower for why this can't use ModifyEnergyGain — the natural per-turn refill
    // bypasses that hook entirely, so the bonus must be applied directly here instead.
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || player.PlayerCombatState == null) return;
        Flash();
        player.PlayerCombatState.GainEnergy(3m);
        if (!HeldNotePower.IsActive(Owner))
        {
            Invariants.Check(Amount > 0, nameof(UnjadedPower) + "." + nameof(AfterEnergyReset),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
