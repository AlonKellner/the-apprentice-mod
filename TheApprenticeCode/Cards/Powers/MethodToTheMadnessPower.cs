using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class MethodToTheMadnessPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Method to the Madness",
        "At the start of your turn, apply [gold]Planned[/gold] to a random card in your hand.",
        "At the start of your turn, choose a card in your hand to apply [gold]Planned[/gold] to.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        bool upgraded = Amount >= 1m;

        if (upgraded)
        {
            var selected = await CardSelectCmd.FromHand(
                context, player,
                new CardSelectorPrefs(
                    new LocString("cards", "THEAPPRENTICE-METHOD_TO_THE_MADNESS.selectionPrompt"), 0, 1),
                c => !c.TryGetModifier<PlannedModifier>(out _),
                this);
            var target = selected?.FirstOrDefault();
            if (target != null)
                PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
        }
        else
        {
            var handCards = player.Piles
                .Where(p => p.Type == PileType.Hand)
                .SelectMany(p => p.Cards)
                .Where(c => !c.TryGetModifier<PlannedModifier>(out _))
                .ToList();
            if (handCards.Count == 0) return;
            var target = handCards[Random.Shared.Next(handCards.Count)];
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
        }
    }
}
