using BaseLib.Abstracts;
using BaseLib.Extensions;
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

    // ── "Decrement only where the effect actually happened" ───────────────────────────────────────
    // House rule for every turn-decaying power: it ticks down immediately after it takes effect, and
    // never on a turn it did nothing. Most powers get this for free by doing the effect and the
    // decrement in the SAME hook — Jaded (AfterEnergyReset), Unshaken (AfterPlayerTurnStart), Shaken
    // (BeforeSideTurnEnd) — so a power granted after that hook already ran simply never decrements.
    //
    // A power whose effect lives in a value-modifier hook cannot do that: ModifyHandDraw is a
    // synchronous pure modifier that may be consulted more than once, and PowerCmd.Decrement is
    // async. Those powers mark from the effect site and consume the mark in their turn hook, which
    // reproduces the same guarantee. Without it the decrement fires unconditionally: Punished grants
    // at AfterPlayerTurnStartLate, the one moment that lands AFTER the turn-start draw but BEFORE the
    // decay hook, so a freshly granted Unlimited lost a stack for a turn it never drew on.
    //
    // Per-combat by construction — power instances don't outlive their combat, and the flag is set
    // and consumed within a single turn. Deliberately not persisted: a mid-turn save/reload loses it,
    // which skips one decrement rather than stealing one, the safe direction to fail in.
    private bool _tookEffectThisTurn;

    protected void MarkTookEffectThisTurn() => _tookEffectThisTurn = true;

    protected bool ConsumeTookEffectThisTurn()
    {
        bool took = _tookEffectThisTurn;
        _tookEffectThisTurn = false;
        return took;
    }
}
