using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Remix : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Remix";

    // Never targeted by the player — every card in the plan gets its own independently
    // randomized target below, so there's nothing for a player-picked target to feed into.
    public Remix() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Planned);
    }

    // Glow gold while there are Planned cards to resolve — same cue as the other Planned resolvers
    // (Performance/CurtainCall/Encore/FinalBar), signalling that playing this now will play the queue.
    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        // Locked once recorded and shuffled here — never re-fetched or re-sorted afterward. See
        // Performance.OnPlay for the full reasoning: a card in this list can itself be a
        // Planned-queue resolver, which can resolve some of the OTHER entries still waiting here as
        // a side effect of its own nested pass — every entry below always gets played regardless,
        // and the per-entry guards just avoid redoing (not re-playing) work another resolver
        // already did.
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        player.RunState.Rng.CombatCardSelection.Shuffle(planned);
        Log.Info($"Remix.OnPlay: playing {planned.Count} Planned slot(s) in shuffled order, each independently retargeted");
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            // Does the card still exist? Real, not hypothetical: base-game Transform cards (e.g.
            // Begone) swap a card's original CardModel object out for a brand-new one, detaching
            // the original from every pile.
            if (card.Pile == null)
            {
                Log.Info($"Remix.OnPlay: {card.Id} is no longer in any pile — skipped");
                continue;
            }

            // RemoveSlot only clears UnplayableModifier once ALL of a card's Planned slots are
            // gone, but a multi-slot card must still be playable on EACH of its own plays in this
            // loop — CardCmd.AutoPlay silently no-ops if the card still carries Unplayable.
            if (card.TryGetModifier<UnplayableModifier>(out var stillUnplayable))
                CardModifier.DirectModifiers(card).Remove(stillUnplayable);

            // Guarded rather than asserted — a nested resolver played from elsewhere in this same
            // locked sequence may have already removed this slot as part of its own pass. Either
            // way, this entry was recorded in the locked sequence, so it always gets played below.
            if (card.TryGetModifier<PlannedModifier>(out var stillPlanned) && stillPlanned.SequenceIndices.Contains(slotSeqIdx))
                PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);

            // Always pass null rather than reusing any target across cards: CardCmd.AutoPlay
            // itself rolls a fresh random living enemy for an AnyEnemy card whenever its target
            // argument is null, so this re-randomizes independently for every single card played,
            // not just when the previous target has died.
            await CardCmd.AutoPlay(context, card, null, AutoPlayType.None, false, false);
        }
        PlannedModifier.InvokeChanged();
    }
}
