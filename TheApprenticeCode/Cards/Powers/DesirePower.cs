using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class DesirePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Desire",
        "Whenever you gain a debuff, add 1 [gold]Dream[/gold] to your hand.",
        "");

    private int _lastWeak;
    private int _lastVul;

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        _lastWeak = Owner.GetPowerAmount<WeakPower>();
        _lastVul = Owner.GetPowerAmount<VulnerablePower>();
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var player = cardPlay.Card.Owner;

        int curWeak = Owner.GetPowerAmount<WeakPower>();
        int curVul = Owner.GetPowerAmount<VulnerablePower>();
        int newWeak = Math.Max(0, curWeak - _lastWeak);
        int newVul = Math.Max(0, curVul - _lastVul);
        int total = newWeak + newVul;

        _lastWeak = curWeak;
        _lastVul = curVul;

        if (total > 0)
            await DreamsAndAmbitions.AddDreams(player, Owner.CombatState!, total);
    }
}
