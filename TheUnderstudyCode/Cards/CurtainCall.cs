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

public class CurtainCall : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CurtainCall";

    public CurtainCall() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Planned);
    }

    // Only require a target when the queue actually has a card that needs one — an empty plan,
    // or a plan of only AoE/self/no-target cards, should just play with no reticle. Owner throws
    // on a canonical (not-yet-instantiated) card model, so fall back to the constructor-seeded
    // value there (bare-construction tests, card library previews, etc).
    public override TargetType TargetType =>
        IsMutable
            ? (PlannedModifier.QueueNeedsEnemyTarget(PlannedModifier.RelevantCards(Owner)) ? TargetType.AnyEnemy : TargetType.None)
            : base.TargetType;

    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var combatState = player.Creature.CombatState!;

        // Locked once recorded here and never re-fetched or re-sorted. See Performance.OnPlay for
        // the full reasoning: a card in this list can itself be a Planned-queue resolver, which can
        // resolve some of the OTHER entries still waiting here as a side effect of its own nested
        // pass — every entry below always gets played regardless, and the per-entry guards just
        // avoid redoing (not re-playing) work another resolver already did.
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        Log.Info($"CurtainCall.OnPlay: playing {planned.Count} Planned slot(s)");
        var currentTarget = cardPlay.Target;
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            // Does the card still exist? Real, not hypothetical: base-game Transform cards (e.g.
            // Begone) swap a card's original CardModel object out for a brand-new one, detaching
            // the original from every pile.
            if (card.Pile == null)
            {
                Log.Info($"CurtainCall.OnPlay: {card.Id} is no longer in any pile — skipped");
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

            // Re-target to a fresh random living enemy if the current one has died partway
            // through the plan, until that one also dies.
            if (card.TargetType == TargetType.AnyEnemy && (currentTarget == null || currentTarget.IsDead))
            {
                var previousTarget = currentTarget;
                currentTarget = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
                Log.Info($"CurtainCall.OnPlay: target {previousTarget?.LogName ?? "(none)"} is no longer available — " +
                          $"re-targeted to {currentTarget?.LogName ?? "(none)"}");
            }

            await CardCmd.AutoPlay(context, card, currentTarget, AutoPlayType.None, false, false);
        }
        PlannedModifier.InvokeChanged();
    }
}
