using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class ObsessionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Obsession",
        "Whenever a card becomes [gold]Unplayable[/gold], gain {Amount} [gold]Block[/gold].",
        "");

    private int _lastPlannedCount;
    private int _lastSpentCount;

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        var allCards = Owner.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>();
        _lastPlannedCount = PlannedModifier.CountIn(allCards);
        _lastSpentCount = allCards.Count(c => c.TryGetModifier<SpentModifier>(out _));
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();

        int currentPlanned = PlannedModifier.CountIn(allCards);
        int currentSpent = allCards.Count(c => c.TryGetModifier<SpentModifier>(out _));

        int newPlanned = Math.Max(0, currentPlanned - _lastPlannedCount);
        int newSpent = Math.Max(0, currentSpent - _lastSpentCount);
        int total = newPlanned + newSpent;

        _lastPlannedCount = currentPlanned;
        _lastSpentCount = currentSpent;

        if (total > 0)
            await CreatureCmd.GainBlock(Owner, (int)Amount * total, ValueProp.Unpowered, null, false);
    }
}
