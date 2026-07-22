using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Timeline;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyCardPool : CustomCardPoolModel
{
    public override string Title => TheUnderstudy.CharacterId;

    // Epoch-gated cards: pruned from rewards/shop until their Timeline epoch is revealed (mirrors the base
    // characters' own pools, e.g. IroncladCardPool). Each epoch's Cards list is unlocked when it reveals.
    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
    {
        var list = cards.ToList();
        Prune<Understudy1Epoch>(list, unlockState, Understudy1Epoch.Cards);
        Prune<Understudy2Epoch>(list, unlockState, Understudy2Epoch.Cards);
        Prune<Understudy3Epoch>(list, unlockState, Understudy3Epoch.Cards);
        Prune<Understudy6Epoch>(list, unlockState, Understudy6Epoch.Cards);
        Prune<Understudy7Epoch>(list, unlockState, Understudy7Epoch.Cards);
        return list;
    }

    private static void Prune<TEpoch>(List<CardModel> list, UnlockState unlockState, List<CardModel> gated)
        where TEpoch : MegaCrit.Sts2.Core.Timeline.EpochModel
    {
        if (unlockState.IsEpochRevealed<TEpoch>()) return;
        list.RemoveAll(c => gated.Any(g => g.Id == c.Id));
    }

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Amber/brass card frame via the base game's HSV recolor shader (res://shaders/hsv.gdshader) —
    // no custom frame art needed. The shader hue-rotates the shared frame texture; H sits just above
    // the base game's orange frame material (h=0.12) toward gold, with V pulled back for a warmer,
    // more metallic brass tone rather than a bright lemon yellow.
    // Tune H in ~0.13–0.20 (lower = more amber/orange, higher = brighter yellow); S = saturation, V = brightness.
    public override float H => 0.14f;
    public override float S => 1.25f;
    public override float V => 1.1f;

    public override Color DeckEntryCardColor => new("c9992e");

    public override bool IsColorless => false;
}
