using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;

// Visual-only companion to OrderModifier — has no logic of its own (same shape as the base game's
// own Smog affliction), purely here so TheUnderstudyCode/Patches/OrderOverlayPatch.cs has an
// AfflictionModel type to key off of (Model.Affliction is Order) when attaching the red shimmer
// overlay. All differentiating text/state lives on OrderModifier instead, since
// AfflictionModel.Title/Description/ExtraCardText are fixed per-type via the "afflictions" loc
// table and can't vary per instance the way a CardModifier's own description hook can.
//
// NOTE: this class deliberately does NOT rely on AfflictionModel.OverlayPath/CreateOverlay() (the
// scene+PreloadManager convention) — that path never produced a visible overlay in-game for
// unknown reasons, even once the scene was correctly named and preloaded. OrderOverlayPatch attaches
// the overlay directly in code instead (GD.Load<Shader> + ShaderMaterial on a plain ColorRect), per
// https://github.com/Alchyr/ModTemplate-StS2/wiki/Shaders.
public sealed class Order : AfflictionModel
{
    public override bool CanAfflictCardType(CardType cardType) =>
        cardType == CardType.Attack || cardType == CardType.Skill;

    public override bool CanAfflict(CardModel card) =>
        base.CanAfflict(card) && !card.IsStable();
}
