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

public class Encore : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Encore";

    public Encore() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
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

        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        int expectedPlays = planned.Count;
        int actualPlays = 0;
        Log.Info($"Encore.OnPlay: playing {expectedPlays} Planned slot(s), re-queuing eligible cards afterward");
        var currentTarget = cardPlay.Target;
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);
            // RemoveSlot only clears UnplayableModifier once ALL of a card's Planned slots are
            // gone, but a multi-slot card must still be playable on EACH of its own plays in this
            // loop — CardCmd.AutoPlay silently no-ops if the card still carries Unplayable.
            if (card.TryGetModifier<UnplayableModifier>(out var stillUnplayable))
                CardModifier.DirectModifiers(card).Remove(stillUnplayable);

            // Re-target to a fresh random living enemy if the current one has died partway
            // through the plan, until that one also dies.
            if (card.TargetType == TargetType.AnyEnemy && (currentTarget == null || currentTarget.IsDead))
            {
                var previousTarget = currentTarget;
                currentTarget = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
                Log.Info($"Encore.OnPlay: target {previousTarget?.LogName ?? "(none)"} is no longer available — " +
                          $"re-targeted to {currentTarget?.LogName ?? "(none)"}");
            }

            await CardCmd.AutoPlay(context, card, currentTarget, AutoPlayType.None, false, false);
            actualPlays++;
            if (PlannedModifier.CanApplyTo(card))
                PlannedModifier.Apply(card, PlannedModifier.RelevantCards(player));
        }
        Invariants.CheckEqual(expectedPlays, actualPlays, nameof(Encore) + "." + nameof(OnPlay),
            "Planned cards auto-played");
        PlannedModifier.InvokeChanged();
    }
}
