using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using System;
using MegaCrit.Sts2.Core.Unlocks;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Relics;
using TheUnderstudy.TheUnderstudyCode.Timeline;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheUnderstudy.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Relics that carry [Pool] only to satisfy the analyzer but are obtained by other means and must
    // never appear as a random reward. The Book of Endings comes from "The Golden Bedroom".
    public static readonly IReadOnlyList<Type> EventOnlyRelics = new[]
    {
        typeof(BookOfEndings),
    };

    // The "Dreamless" epoch (ep1) gates 3 relics until it is revealed; event-only relics are always excluded.
    public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
    {
        var relics = base.GetUnlockedRelics(unlockState).ToList();
        relics.RemoveAll(r => EventOnlyRelics.Contains(r.GetType()));
        if (!unlockState.IsEpochRevealed<Understudy1Epoch>())
            relics.RemoveAll(r => Understudy1Epoch.Relics.Any(g => g.Id == r.Id));
        return relics;
    }
}
