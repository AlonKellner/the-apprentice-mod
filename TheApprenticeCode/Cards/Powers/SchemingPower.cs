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
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class SchemingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Scheming",
        "At the start of your turn, choose a card in your hand to apply [gold]Planned[/gold] to.",
        "At the start of your turn, choose a card in your hand to apply [gold]Planned[/gold] to.");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        var eligible = player.Piles
            .Where(p => p.Type == PileType.Hand)
            .SelectMany(p => p.Cards)
            .Where(c => PlannedModifier.CanApplyTo(c))
            .ToList();

        if (eligible.Count == 0) return;

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(
                new LocString("cards", "THEAPPRENTICE-SCHEMING.selectionPrompt"), 1, 1),
            c => PlannedModifier.CanApplyTo(c),
            this);
        var target = selected?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, allCards);
    }
}
