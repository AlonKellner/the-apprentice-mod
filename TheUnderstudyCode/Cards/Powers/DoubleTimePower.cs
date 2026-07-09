using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class DoubleTimePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Matches base-game AccelerantPower's own two-tier description exactly: the plain
    // "description" (shown via GetDumbHoverTip when another card previews this Power, and stays
    // static/singular there) versus "smartDescription" (the live in-combat tooltip, dynamic on
    // Amount via SmartFormat's built-in `plural` formatter — {Amount:plural:time|times}).
    public override List<(string, string)> Localization => new PowerLoc(
        "Double Time",
        "All [gold]invertible[/gold] buff and debuff gains are applied an additional time.",
        "All [gold]invertible[/gold] buff and debuff gains are applied [blue]{Amount}[/blue] additional {Amount:plural:time|times}.");

    internal static bool IsInvertiblePower(PowerModel power) =>
        power is WeakPower or UnweakPower
            or VulnerablePower or UnvulnerablePower
            or ShakenPower or UnshakenPower
            or LimitedPower or UnlimitedPower
            or JadedPower or UnjadedPower
            or FrailPower or UnfrailPower
            or StrengthPower or DexterityPower;

    // Re-entrancy guard: each extra application below fires this same broadcast hook again (it's
    // a real ModifyAmount call, indistinguishable from an organic one), so this stops those from
    // ALSO trying to trigger further repeats — otherwise it recurses forever. Only the original,
    // outermost application should ever start a repeat sequence. Also gates the raw-amount
    // capture below, so a repeat's own BeforePowerAmountChanged doesn't overwrite the value
    // captured for the outermost event it's still replaying.
    private bool _isRepeating;

    // The amount an Un-X power's own bidirectional interception (see UnfrailPower et al.) ends up
    // applying can be reduced from what was actually offered, live, against whatever opposing
    // debuff/buff stock exists at that moment. Repeating that already-reduced amount
    // (AfterPowerAmountChanged's own `amount` parameter) would under-count how much cancellation
    // each repeat should independently re-check for itself — this is exactly the mechanism that
    // produces a differentiated total (e.g. 3 vs 5 Unfrail) instead of a flat multiple. So the
    // RAW, pre-interception amount is captured here (BeforePowerAmountChanged fires before
    // interception runs) and replayed for every repeat, letting each one re-run interception fresh.
    private decimal? _capturedRawAmount;

    public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
    {
        if (!_isRepeating && applier == Owner && amount > 0m && IsInvertiblePower(power))
        {
            Invariants.Check(_capturedRawAmount == null, nameof(DoubleTimePower) + "." + nameof(BeforePowerAmountChanged),
                $"a previous capture ({_capturedRawAmount}) was never consumed before a new invertible gain started — nested/overlapping capture");
            _capturedRawAmount = amount;
        }
        return Task.CompletedTask;
    }

    // Repeats the gain itself rather than scaling its amount — the same idiom base-game
    // Accelerant uses (it makes Poison's damage trigger fire additional times; it does not scale
    // Poison's damage per trigger). This matters here because other reactive effects (e.g. Full
    // Voice's "whenever a debuff is applied to you") need to see N separate applications, not one
    // application of a bigger number: apply 1 Weak with Double Time (Amount 1) active should apply
    // 1 Weak twice, not 2 Weak once.
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_isRepeating || applier != Owner || amount <= 0m || !IsInvertiblePower(power)) return;

        Invariants.Check(_capturedRawAmount != null, nameof(DoubleTimePower) + "." + nameof(AfterPowerAmountChanged),
            "fired for an invertible gain with no raw amount captured by BeforePowerAmountChanged — hook " +
            "ordering assumption broke; falling back to the post-interception amount, which may under-count repeats.");
        decimal rawAmount = _capturedRawAmount ?? amount;
        _capturedRawAmount = null;

        _isRepeating = true;
        try
        {
            for (int i = 0; i < Amount && power.Owner != null; i++)
                await PowerCmd.ModifyAmount(choiceContext, power, rawAmount, applier, cardSource);
        }
        finally
        {
            _isRepeating = false;
        }
    }
}
