using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Timeline;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheUnderstudy.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // The "Consumed" epoch (ep4) gates 3 relics until it is revealed.
    public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
    {
        var relics = base.GetUnlockedRelics(unlockState).ToList();
        if (!unlockState.IsEpochRevealed<Understudy4Epoch>())
            relics.RemoveAll(r => Understudy4Epoch.Relics.Any(g => g.Id == r.Id));
        return relics;
    }
}
