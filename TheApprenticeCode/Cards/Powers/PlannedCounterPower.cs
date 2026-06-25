using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class PlannedCounterPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Planned",
        "You have {Amount} [gold]Planned[/gold] card(s) across all your piles.",
        "");

    public override int DisplayAmount => PlannedModifier.CountIn(
        Owner?.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>());

    protected override bool IsVisibleInternal => true;

    private int _lastCount = 0;

    private void UpdateDisplayIfChanged()
    {
        var allCards = Owner?.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>();
        PlannedModifier.RefreshVisualIndices(allCards);
        var current = PlannedModifier.CountIn(allCards);
        if (current == _lastCount) return;
        _lastCount = current;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        PlannedModifier.Changed += UpdateDisplayIfChanged;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        PlannedModifier.Changed -= UpdateDisplayIfChanged;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        UpdateDisplayIfChanged();
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        UpdateDisplayIfChanged();
        await Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        UpdateDisplayIfChanged();
        await Task.CompletedTask;
    }
}
