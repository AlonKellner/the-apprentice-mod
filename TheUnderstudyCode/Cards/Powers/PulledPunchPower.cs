using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class PulledPunchPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Pulled Punch",
        "Whenever an [gold]invertible[/gold] debuff is applied to you, reduce its value by 1 immediately after it lands.",
        "Whenever an [gold]invertible[/gold] debuff is applied to you, reduce its value by {Amount} immediately after it lands.");

    public static int ComputeDebuffPairSteps(PowerType type, decimal landedAmount, int currentAmount, int reduceBy) =>
        type != PowerType.Debuff || landedAmount <= 0m ? 0 : Math.Min(reduceBy, currentAmount);

    public static int ComputeSignFlipSteps(decimal landedAmount, int currentAmount, int reduceBy) =>
        landedAmount >= 0m || currentAmount >= 0 ? 0 : Math.Min(reduceBy, -currentAmount);

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner || !DoubleTimePower.IsInvertiblePower(power)) return;

        bool isSignFlip = power is StrengthPower or DexterityPower;
        int steps = isSignFlip
            ? ComputeSignFlipSteps(amount, power.Amount, Amount)
            : ComputeDebuffPairSteps(power.Type, amount, power.Amount, Amount);

        // A correction that lands a debuff-pair power exactly at 0 causes the outer, original
        // PowerCmd.ModifyAmount call (the one for the landing application itself) to also see
        // Amount <= 0 and call Remove a second time once this hook returns. Harmless: none of the
        // 8 invertible power types override AfterRemoved, so the duplicate removal is a no-op.
        if (isSignFlip)
            for (int i = 0; i < steps && power.Owner != null; i++)
                await PowerCmd.ModifyAmount(choiceContext, power, 1m, applier, cardSource);
        else
            for (int i = 0; i < steps && power.Owner != null; i++)
                await PowerCmd.Decrement(power);
    }
}
