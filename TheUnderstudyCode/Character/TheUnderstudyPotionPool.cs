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
    // The compendium (Potion Lab) tints each potion tile's shadow to this (at 66% alpha). White reads as
    // invisible, so use the character's gold accent — base-game characters all use a saturated color here.
    public override Color LabOutlineColor => TheUnderstudy.GoldColor;

    // Mark the character's potions "seen" so the Potion Lab shows them in full — with the gold shadow above
    // — like base-game character potions, instead of as dark "unknown" tiles (which never get the color).
    // (Epoch-gated potions still show Locked until "Consumed" is revealed; Locked takes precedence.)
    public override bool SeenByDefault => true;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // The "Consumed" epoch (ep4) gates 3 potions until it is revealed.
    public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
    {
        var potions = base.GetUnlockedPotions(unlockState).ToList();
        if (!unlockState.IsEpochRevealed<Understudy4Epoch>())
            potions.RemoveAll(p => Understudy4Epoch.Potions.Any(g => g.Id == p.Id));
        return potions;
    }
}
