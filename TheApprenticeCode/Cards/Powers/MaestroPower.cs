using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class MaestroPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Maestro",
        "Whenever you apply [gold]Planned[/gold] to a card, draw 1 card.",
        "Whenever you apply [gold]Planned[/gold] to a card, draw 1 card.");

    private int _lastPlannedCount;

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        var allCards = Owner.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>();
        _lastPlannedCount = PlannedModifier.CountIn(allCards);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards);

        int currentPlanned = PlannedModifier.CountIn(allCards);
        int newPlanned = Math.Max(0, currentPlanned - _lastPlannedCount);
        _lastPlannedCount = currentPlanned;

        for (int i = 0; i < newPlanned; i++)
            await CardPileCmd.Draw(context, 1, player, false);
    }
}
