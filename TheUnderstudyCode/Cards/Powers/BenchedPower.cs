using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class BenchedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Benched",
        "Whenever a card becomes [gold]Unplayable[/gold], gain {Amount} [gold]Block[/gold].",
        "Whenever a card becomes [gold]Unplayable[/gold], gain {Amount} [gold]Block[/gold].");

    private int _lastPlannedCount;
    private int _lastUnplayableCount;

    private static int CountUnplayable(IEnumerable<CardModel> cards) =>
        cards.Count(c => c.TryGetModifier<UnplayableModifier>(out _));

    public override Task AfterApplied(Creature? creature, CardModel? card)
    {
        var allCards = (Owner.Player?.Piles.SelectMany(p => p.Cards) ?? Enumerable.Empty<CardModel>()).ToList();
        _lastPlannedCount = PlannedModifier.CountIn(allCards);
        _lastUnplayableCount = CountUnplayable(allCards);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        var allCards = cardPlay.Card.Owner.Piles.SelectMany(p => p.Cards).ToList();

        int newPlanned = Math.Max(0, PlannedModifier.CountIn(allCards) - _lastPlannedCount);
        int intenseSpent = IntenseModifier.IsFinalIntensePlay(cardPlay) ? 1 : 0;
        int total = newPlanned + intenseSpent;

        _lastPlannedCount = PlannedModifier.CountIn(allCards);
        _lastUnplayableCount = CountUnplayable(allCards);

        if (total > 0)
            await CreatureCmd.GainBlock(Owner, (int)Amount * total, ValueProp.Unpowered, null, false);
    }

    // Catches Unplayable applied outside any card play — e.g. ShakenPower's mass hand-wide
    // application in its own BeforeSideTurnEnd, which AfterCardPlayed never observes since no
    // card was played to trigger it.
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;
        var allCards = Owner.Player.Piles.SelectMany(p => p.Cards).ToList();

        int newUnplayable = Math.Max(0, CountUnplayable(allCards) - _lastUnplayableCount);
        _lastUnplayableCount = CountUnplayable(allCards);

        if (newUnplayable > 0)
            await CreatureCmd.GainBlock(Owner, (int)Amount * newUnplayable, ValueProp.Unpowered, null, false);
    }

    // Resyncs after turn-start removals (e.g. UnshakenPower stripping Unplayable) so a later
    // AfterSideTurnEnd diff isn't masked by a stale, too-high baseline on a turn with no card
    // plays in between. The Late variant runs after UnshakenPower's own (non-Late)
    // AfterPlayerTurnStart removal, so it always captures the post-removal count.
    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        _lastUnplayableCount = CountUnplayable(player.Piles.SelectMany(p => p.Cards));
        return Task.CompletedTask;
    }
}
