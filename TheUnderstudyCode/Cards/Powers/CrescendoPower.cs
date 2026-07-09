using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class CrescendoPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Crescendo",
        "Double all gained [gold]Vigor[/gold].",
        "Double all gained [gold]Vigor[/gold], {Amount:plural:time|times}.");

    public static decimal ComputeMultiplier(int stacks) => (decimal)Math.Pow(2, stacks);

    public override decimal ModifyPowerAmountGivenMultiplicative(
        PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource) =>
        power is VigorPower && giver == Owner ? ComputeMultiplier(Amount) : 1m;
}
