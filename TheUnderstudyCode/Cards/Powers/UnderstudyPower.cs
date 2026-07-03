using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Shared base for every custom Power in this deck, mirroring UnderstudyCard's PortraitPath
// pattern: each power automatically gets an icon path derived from its own Id, falling back to a
// placeholder image (via PowerImagePath()/BigPowerImagePath()'s own ResourceLoader.Exists check)
// when no power-specific art has been made yet — instead of no icon/sprite at all.
public abstract class UnderstudyPower : CustomPowerModel
{
    public override string? CustomPackedIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();

    public override string? CustomBigIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    // NPowerContainer (the game's own power-tray UI, confirmed by decompiling it) only ever
    // creates a power's visual badge at the moment it's first Applied (Creature.PowerApplied),
    // checking IsVisible exactly once at that instant — it never listens for amount changes
    // afterward. A power that's attached while hidden (the always-present Un-X powers, which need
    // to exist from combat start so their own cancellation hooks can catch a debuff's very first
    // gain — see EmotionalExpression.EnsureAttached) and only later becomes visible would
    // otherwise never get a badge at all, no matter what its Amount is. Detect exactly that
    // transition and force a fresh PowerApplied once IsVisible actually flips true, by directly
    // toggling this instance's membership in Owner's power list — NOT via PowerCmd.Remove/Apply,
    // which would re-run interception and Fortissimo's repeat hook against the very Amount this
    // is only trying to make visible. Powers that are already visible at attach time (i.e.
    // everything except the always-present Un-X powers) are untouched by this — _wasHiddenAtAttach
    // stays false for them, so the check below never fires.
    private bool _wasHiddenAtAttach;
    private bool _shownOnceVisible;
    private bool _isRefreshingVisibility;

    // Suppresses the "power removed" flash VFX (NCreature.OnPowerRemoved plays one whenever
    // Creature.PowerRemoved fires and ShouldPlayVfx is true) specifically for the synthetic
    // Remove+Apply toggle below — everywhere else this behaves exactly like the base game default.
    public override bool ShouldPlayVfx => !_isRefreshingVisibility && base.ShouldPlayVfx;

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        _wasHiddenAtAttach = !IsVisible;
        return base.AfterApplied(creature, card);
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_wasHiddenAtAttach && !_shownOnceVisible && ReferenceEquals(power, this) && IsVisible && Owner != null)
        {
            _shownOnceVisible = true;
            var owner = Owner;
            // Deferred (not synchronous): this fires from inside the same broadcast loop that's
            // iterating every power's AfterPowerAmountChanged right now — mutating Owner's power
            // list mid-iteration risks a "collection modified" failure. CallDeferred is the same
            // pattern the engine's own power-VFX code uses for scheduling follow-up node/tree
            // mutations out of a power-event handler (see NCreature.OnPowerIncreased/OnPowerRemoved).
            Callable.From(() =>
            {
                if (!owner.Powers.Contains(this) || !IsVisible) return;
                _isRefreshingVisibility = true;
                owner.RemovePowerInternal(this);
                owner.ApplyPowerInternal(this);
                _isRefreshingVisibility = false;
            }).CallDeferred();
        }
        return base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }
}
