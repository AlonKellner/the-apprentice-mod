using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Timeline;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => TheUnderstudy.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // The "Taken" epoch (ep5) gates 3 potions until it is revealed.
    public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
    {
        var potions = base.GetUnlockedPotions(unlockState).ToList();
        if (!unlockState.IsEpochRevealed<Understudy5Epoch>())
            potions.RemoveAll(p => Understudy5Epoch.Potions.Any(g => g.Id == p.Id));
        return potions;
    }
}
