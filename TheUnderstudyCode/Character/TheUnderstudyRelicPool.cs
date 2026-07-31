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

    // The "Dreamless" epoch (ep1) gates 3 relics until it is revealed. The Book of Endings (Event rarity,
    // obtained from "The Golden Bedroom") is deliberately NOT removed here: it must stay in the unlocked
    // set so the compendium shows it as a hidden "unknown" tile that becomes visible once obtained — the
    // base-game Event-relic behaviour. It can never drop as a random reward regardless, because every
    // reward path (RelicGrabBag) rarity-filters to Common/Uncommon/Rare/Shop, and Event is not among them.
    public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
    {
        var relics = base.GetUnlockedRelics(unlockState).ToList();
        if (!unlockState.IsEpochRevealed<Understudy1Epoch>())
            relics.RemoveAll(r => Understudy1Epoch.Relics.Any(g => g.Id == r.Id));
        return relics;
    }
}
