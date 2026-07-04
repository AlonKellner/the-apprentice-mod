using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class StandingByPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Set true once an upgraded Standing By has been played this combat. Sticky rather than
    // reset by later unupgraded copies — once the player has access to the smarter version,
    // it stays available.
    public bool ChoiceMode { get; set; }

    // PowerLoc's 2nd/3rd args are (Description, SmartDescription) — the "dumb" tip shown when
    // another card previews this Power vs. the live in-combat tooltip on the actual applied
    // instance — NOT a base/upgraded pair. Both need to reflect the live ChoiceMode here (the
    // card's own cards.json description already handles the base-vs-upgraded preview via
    // {IfUpgraded:show:...}), otherwise the in-combat tooltip would always claim "of your choice"
    // even on an unupgraded (still-random) instance.
    private string DescriptionText => ChoiceMode
        ? "Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 attack or skill of your choice in hand."
        : "Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 random attack or skill in hand.";

    public override List<(string, string)> Localization => new PowerLoc("Standing By", DescriptionText, DescriptionText);

    private readonly List<CardModel> _pending = new();

    public override Task AfterApplied(Creature? creature, CardModel? cardSource)
    {
        UnplayableModifier.Applied += OnUnplayableApplied;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        UnplayableModifier.Applied -= OnUnplayableApplied;
        return Task.CompletedTask;
    }

    private void OnUnplayableApplied(CardModel card)
    {
        if (card.Owner?.Creature == Owner) _pending.Add(card);
    }

    // Checked once per real card play — by then every UnplayableModifier attached during that
    // play's resolution (Planned/Intense included) has already fired OnUnplayableApplied above.
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (_pending.Count == 0 || Owner?.Player == null) return;
        var triggers = _pending.ToList();
        _pending.Clear();

        Invariants.Check(triggers.Distinct().Count() == triggers.Count,
            nameof(StandingByPower) + "." + nameof(AfterCardPlayed),
            "the pending trigger queue has duplicate cards — OnUnplayableApplied fired twice for the same card");

        var player = Owner.Player;
        foreach (var triggeringCard in triggers)
        {
            Invariants.Check(triggeringCard.TryGetModifier<UnplayableModifier>(out _),
                nameof(StandingByPower) + "." + nameof(AfterCardPlayed),
                $"{triggeringCard.Id} queued this trigger by becoming Unplayable, but no longer carries UnplayableModifier by drain time");

            var candidates = PileType.Hand.GetPile(player).Cards
                .Where(c => c != triggeringCard && UnplayableModifier.CanApplyTo(c))
                .ToList();
            if (candidates.Count == 0) continue;

            if (ChoiceMode)
            {
                var selected = await CardSelectCmd.FromHand(
                    context,
                    player,
                    new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-STANDING_BY.selectionPrompt"), 0, 1),
                    c => candidates.Contains(c),
                    this);
                var chosen = selected?.FirstOrDefault();
                if (chosen != null) UnplayableModifier.Remove(chosen);
            }
            else
            {
                var chosen = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
                if (chosen != null) UnplayableModifier.Remove(chosen);
            }
        }
    }
}
