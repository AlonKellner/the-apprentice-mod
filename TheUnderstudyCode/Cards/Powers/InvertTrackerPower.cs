using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Silently auto-attached to the player at combat start (see UnderstudyCard.AfterPlayerTurnStartLate,
// mirroring PlannedCounterPower's own auto-attach). Its only job is observing every power-amount
// change on the player via the global AfterPowerAmountChanged broadcast hook, and feeding
// EmotionalExpression's "last modified invertible debuff" tracker — this is what lets an
// enemy-inflicted (or relic-inflicted, or any other externally-caused) Weak/Vulnerable/etc.
// register as "last modified" for Invert, not just this deck's own Apply/Convert calls.
public class InvertTrackerPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override List<(string, string)> Localization => new PowerLoc("Invert Tracker", "", "");

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || amount == 0m) return Task.CompletedTask;
        var debuff = MapToInvertibleDebuff(power);
        if (debuff != null) EmotionalExpression.RecordModified(Owner, debuff.Value);
        return Task.CompletedTask;
    }

    private static InvertibleDebuff? MapToInvertibleDebuff(PowerModel power) => power switch
    {
        WeakPower or UnweakPower => InvertibleDebuff.Weak,
        VulnerablePower or UnvulnerablePower => InvertibleDebuff.Vulnerable,
        ShakenPower or UnshakenPower => InvertibleDebuff.Shaken,
        LimitedPower or UnlimitedPower => InvertibleDebuff.Limited,
        JadedPower or UnjadedPower => InvertibleDebuff.Jaded,
        FrailPower or UnfrailPower => InvertibleDebuff.Frail,
        StrengthPower => InvertibleDebuff.Strength,
        DexterityPower => InvertibleDebuff.Dexterity,
        _ => null
    };
}
