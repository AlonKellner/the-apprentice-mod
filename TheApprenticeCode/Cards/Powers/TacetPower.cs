using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class TacetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Tacet",
        "Whenever a card becomes [gold]Unplayable[/gold], gain {Amount} [gold]Block[/gold].",
        "Whenever a card becomes [gold]Unplayable[/gold], gain {Amount} [gold]Block[/gold].");

    private int _lastPlannedCount;
    private int _lastIntenseSpentCount;

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        var allCards = Owner.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>();
        Snapshot(allCards);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var allCards = cardPlay.Card.Owner.Piles.SelectMany(p => p.Cards).ToList();

        int newPlanned = Math.Max(0, PlannedModifier.CountIn(allCards) - _lastPlannedCount);
        int newIntenseSpent = Math.Max(0, CountIntenseSpent(allCards) - _lastIntenseSpentCount);
        int total = newPlanned + newIntenseSpent;

        Snapshot(allCards);

        if (total > 0)
            await CreatureCmd.GainBlock(Owner, (int)Amount * total, ValueProp.Unpowered, null, false);
    }

    private void Snapshot(IEnumerable<CardModel> allCards)
    {
        var list = allCards.ToList();
        _lastPlannedCount = PlannedModifier.CountIn(list);
        _lastIntenseSpentCount = CountIntenseSpent(list);
    }

    private static int CountIntenseSpent(IEnumerable<CardModel> cards) =>
        cards.Count(c => c.TryGetModifier<IntenseModifier>(out var m) && m.IsSpent);
}
