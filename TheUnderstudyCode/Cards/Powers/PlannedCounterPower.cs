using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class PlannedCounterPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new StringVar("CardList") };

    private static readonly string BaseDescription =
        "Your [gold]Planned[/gold] cards across all piles:{CardList}";

    public override List<(string, string)> Localization => new PowerLoc(
        "Planned",
        BaseDescription,
        BaseDescription);

    public override int DisplayAmount => IsMutable
        ? PlannedModifier.CountIn(PlannedModifier.RelevantCards(Owner?.Player))
        : 0;

    protected override bool IsVisibleInternal => true;

    private string _lastPlanList = "";

    public static string BuildPlanList(IEnumerable<string> titles)
    {
        var items = titles.ToList();
        return items.Count == 0
            ? ""
            : "\n" + string.Join('\n', items.Select((t, i) => $"  [gold]#{i + 1}[/gold] {t}"));
    }

    private void UpdateDisplayIfChanged()
    {
        var allCards = PlannedModifier.RelevantCards(Owner?.Player).ToList();
        PlannedModifier.RefreshVisualIndices(allCards);
        var sorted = PlannedModifier.GetSorted(allCards);
        var current = BuildPlanList(sorted.Select(x => x.card.Title)); // one entry per slot; multi-slot cards appear N times
        if (current == _lastPlanList) return;
        _lastPlanList = current;
        ((StringVar)DynamicVars["CardList"]).StringValue = current;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        PlannedModifier.Changed += UpdateDisplayIfChanged;
        // This power is applied lazily (UnderstudyCard.AfterPlayerTurnStartLate, on the first
        // turn it's missing), well after Performance may already have queued cards with
        // PlannedModifier attached this combat. Subscribing alone only catches *future*
        // Changed events — sync with whatever's already Planned right now, or the card list stays
        // empty until something else happens to change Planned state.
        UpdateDisplayIfChanged();
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
