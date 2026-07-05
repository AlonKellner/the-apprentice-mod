using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;

// Visual-only companion to OrderModifier — has no logic of its own (same shape as the base game's
// own Smog affliction), purely here to unlock AfflictionModel.OverlayPath/CreateOverlay() for the
// golden shimmer overlay. All differentiating text/state lives on OrderModifier instead, since
// AfflictionModel.Title/Description/ExtraCardText are fixed per-type via the "afflictions" loc
// table and can't vary per instance the way a CardModifier's own description hook can.
//
// NOTE: AfflictionModel.OverlayPath is NOT virtual — it's always
// "res://scenes/cards/overlays/afflictions/" + Id.Entry.ToLowerInvariant() + ".tscn", so there is
// no override here. Id.Entry = StringHelper.Slugify(type.Name) with no mod-namespace prefix, so
// for this class it resolves to "order" — the scene lives at
// scenes/cards/overlays/afflictions/order.tscn (see generate_order_overlay.py).
public sealed class Order : AfflictionModel
{
    public override bool CanAfflictCardType(CardType cardType) =>
        cardType == CardType.Attack || cardType == CardType.Skill;

    public override bool CanAfflict(CardModel card) =>
        base.CanAfflict(card) && !card.Keywords.Contains(UnderstudyKeywords.Stable);
}
