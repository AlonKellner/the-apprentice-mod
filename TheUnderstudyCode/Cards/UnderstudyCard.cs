using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

[Pool(typeof(TheUnderstudyCardPool))]
public abstract class UnderstudyCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true)
    : ConstructedCardModel(cost, type, rarity, target, showInCardLibrary)
{
    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    // Call in subclass constructors to register the dynamic Intense hover tip. The lambda is
    // evaluated at hover time with the live card, so it reads the current IntenseModifier.Stacks —
    // returns nothing when Intense hasn't been applied yet (no tip clutter on fresh cards).
    protected void WithIntenseTip()
    {
        WithTips(card =>
        {
            if (!card.TryGetModifier<IntenseModifier>(out var mod) || mod.Stacks <= 0)
                return Enumerable.Empty<IHoverTip>();
            int s = mod.Stacks;
            return new IHoverTip[]
            {
                new HoverTip(
                    new LocString("card_keywords", "THEUNDERSTUDY-INTENSE.title"),
                    $"Deal {s} additional damage and gain {s} additional [gold]Block[/gold] for each card with [gold]Intense[/gold]."
                )
            };
        });
    }

    // Snapshot of DirectModifiers at combat start; null means this card is not Stable.
    private List<CardModifier>? _stableSnapshot;

    public override Task BeforeCombatStart()
    {
        var t = base.BeforeCombatStart();
        if (Keywords.Contains(UnderstudyKeywords.Stable))
            _stableSnapshot = CardModifier.DirectModifiers(this).ToList();
        return t;
    }

    // Auto-attach the shared PlannedCounterPower so the queue UI badge is visible whenever
    // Performance queues cards, and the hidden InvertTrackerPower so Invert can react to
    // enemy-inflicted (not just self-applied) invertible debuffs and perform its bidirectional
    // debuff/buff cancellation for all 6 pairs (see InvertTrackerPower for why that logic lives
    // there rather than on each Un-X power).
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        RestoreIfStable();
        if (player != Owner) return;
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
        if (!player.Creature.Powers.Any(p => p is InvertTrackerPower))
            await PowerCmd.Apply<InvertTrackerPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    // Restore on every card-play and turn boundary so no window exists where a Stable card
    // appears modified — covers enemy-applied effects (Ethereal, etc.) that slip past the
    // CanApplyTo guards.
    public override Task AfterCardEnteredCombat(CardModel triggeredBy)
    {
        RestoreIfStable();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        RestoreIfStable();
        // Safe to mutate DirectModifiers here: AfterCardPlayed fires once BaseLib's per-modifier
        // OnPlay enumeration (BeforeAfterPlayHooks) has finished for this play, unlike a
        // CardModifier's own OnPlay override, which runs inside that enumeration and throws
        // "Collection was modified" if it tries to add a modifier to the same card.
        // IsFinalIntensePlay gates this to the last CardPlay in a Replay series, so a card with
        // Replay N only becomes Unplayable after all N+1 plays have resolved.
        if (cardPlay.Card == this
            && IntenseModifier.IsFinalIntensePlay(cardPlay)
            && !this.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(this);
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        RestoreIfStable();
        return Task.CompletedTask;
    }

    private void RestoreIfStable()
    {
        if (_stableSnapshot == null) return;
        var mods = CardModifier.DirectModifiers(this);
        var toRemove = mods.Where(m => !_stableSnapshot.Contains(m)).ToList();
        foreach (var m in toRemove) mods.Remove(m);
        var toRestore = _stableSnapshot.Where(m => !mods.Contains(m)).ToList();
        foreach (var m in toRestore) mods.Add(m);
    }
}
