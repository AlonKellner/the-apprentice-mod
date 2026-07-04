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

public class JadedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Cards that reference this Power via WithTip(typeof(JadedPower)) render it through
    // PowerModel.GetDumbHoverTip, whose variable set is hardcoded to Amount/singleStarIcon/
    // energyPrefix only — it does NOT call DynamicVars.AddTo, so a custom CanonicalVars
    // (e.g. an EnergyVar formatted with {Energy:energyIcons()}) fails there with "No source
    // extension could handle the selector named Energy". energyPrefix IS present in that set,
    // and EnergyIconsFormatter also accepts a plain string value with the icon count passed as
    // a literal formatter argument — {energyPrefix:energyIcons(1)} — so the count no longer
    // needs to be baked into the string via manual repetition.
    public override List<(string, string)> Localization => new PowerLoc(
        "Jaded",
        "Lose {energyPrefix:energyIcons(1)} at the start of your next turn.",
        "Lose {energyPrefix:energyIcons(1)} at the start of your next turn.");

    // The natural per-turn energy refill (PlayerCombatState.ResetEnergy/AddMaxEnergyToCurrent,
    // called directly from CombatManager.SetupPlayerTurn) is a raw field mutation that bypasses
    // PlayerCmd.GainEnergy entirely, so ModifyEnergyGain never actually fires for it — that hook
    // only covers explicit mid-combat "gain N energy" effects (potions, relics, cards). Matches
    // the base game's own pattern for this (see LightningRodPower): adjust Energy directly in
    // AfterEnergyReset, right after the natural reset has already happened this turn.
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || player.PlayerCombatState == null) return;
        Flash();
        player.PlayerCombatState.LoseEnergy(Math.Min(1m, player.PlayerCombatState.Energy));
        if (!HeldNotePower.IsActive(Owner))
        {
            Invariants.Check(Amount > 0, nameof(JadedPower) + "." + nameof(AfterEnergyReset),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
