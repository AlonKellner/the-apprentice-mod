using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class VirtuosoPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Virtuoso",
        Amount == 1
            ? "At the [i]start[/i] of your turn, remove [gold]Unplayable[/gold] from all cards in your hand."
            : "At the end of your turn, remove [gold]Unplayable[/gold] from all cards in your hand.",
        Amount == 1
            ? "At the [i]start[/i] of your turn, remove [gold]Unplayable[/gold] from all cards in your hand."
            : "At the end of your turn, remove [gold]Unplayable[/gold] from all cards in your hand.");

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext context, CombatSide side, System.Collections.Generic.IEnumerable<Creature> creatures)
    {
        if (Amount == 0 && side == CombatSide.Player)
            RemoveUnplayableFromHand();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (Amount == 1 && player == Owner.Player)
            RemoveUnplayableFromHand();
        return Task.CompletedTask;
    }

    private void RemoveUnplayableFromHand()
    {
        var hand = Owner.Player!.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList())
            if (card.TryGetModifier<UnplayableModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);
    }
}
