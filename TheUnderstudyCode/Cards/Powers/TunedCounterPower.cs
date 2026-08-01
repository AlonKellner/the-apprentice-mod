using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// The Tuned analogue of PlannedCounterPower: an always-visible buff that lists the player's currently
// Tuned cards. Unlike Planned (which has a meaningful queue order and numbered slots), Tuned order does
// not matter, so this lists the card names without indices. The badge number is the current per-stack
// Tuned bonus — i.e. how much each Tuned card adds to its Block/damage — which is exactly the number of
// Tuned cards (each Tuned card grants +1 per Tuned card; see TunedModifier.Bonus = Stacks * count).
public class TunedCounterPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // The dedicated Tuned icon (tuned_power.png).
    public override string? CustomPackedIconPath => "tuned_power.png".PowerImagePath();
    public override string? CustomBigIconPath => "tuned_power.png".BigPowerImagePath();

    private const string EmptyHeader = "You have no [gold]Tuned[/gold] cards";
    private const string NonEmptyHeader = "Your [gold]Tuned[/gold] cards:";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new StringVar("Header", EmptyHeader), new StringVar("CardList") };

    // The badge = the current Tuned bonus each Tuned card adds to its Block/damage = the live count of
    // Tuned cards (TunedModifier.TunedCards.Count()). Zero on a canonical/bare power.
    public override int DisplayAmount => IsMutable ? TunedModifier.TunedCards(Owner?.Player).Count() : 0;

    // Always visible ONCE GRANTED — and the grant is what's gated. This power is not applied until the
    // player actually has a Tuned card (see UnderstudyCard.GrantCountersIfNeeded), which also makes it
    // latch: nothing removes it for the rest of the combat, so the counter does not blink out when the
    // last Tuned card is exhausted.
    //
    // Gating the grant rather than this property is deliberate: NPowerContainer.Add creates a power's
    // icon node only if IsVisible at the instant PowerApplied fires, and never re-checks. A power
    // granted while invisible could therefore never appear later, however the flag changed.
    protected override bool IsVisibleInternal => true;

    // null (not "") so the first UpdateDisplayIfChanged always runs and sets both vars explicitly.
    private string? _lastList = null;

    // Indexless: one line per Tuned card name, in whatever order the piles enumerate (order is not
    // meaningful for Tuned, unlike Planned's numbered queue).
    public static string BuildTunedList(IEnumerable<string> titles)
    {
        var items = titles.ToList();
        return items.Count == 0 ? "" : "\n" + string.Join('\n', items.Select(t => $"  {t}"));
    }

    private void UpdateDisplayIfChanged()
    {
        var titles = TunedModifier.TunedCards(Owner?.Player).Select(c => c.Title);
        var current = BuildTunedList(titles);
        if (current == _lastList) return;
        _lastList = current;
        ((StringVar)DynamicVars["Header"]).StringValue = current.Length == 0 ? EmptyHeader : NonEmptyHeader;
        ((StringVar)DynamicVars["CardList"]).StringValue = current;
        InvokeDisplayAmountChanged();
    }

    // TunedModifier.Applied fires on a card's first Tuned; the per-play/turn hooks below catch the rest
    // (clones that gain Tuned behind Apply's back, cards entering/leaving via play or exhaust).
    private void OnTunedApplied(CardModel card) => UpdateDisplayIfChanged();

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        TunedModifier.Applied += OnTunedApplied;
        // Applied lazily (UnderstudyCard.AfterPlayerTurnStartLate), so sync with cards already Tuned now
        // rather than only catching future applications.
        UpdateDisplayIfChanged();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        TunedModifier.Applied -= OnTunedApplied;
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
