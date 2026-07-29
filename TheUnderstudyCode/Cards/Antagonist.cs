using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The understudy who volunteers to be the villain: soak the whole cast's afflictions onto yourself, then
// your Swap/Invert kit turns them into value. Takes every debuff you could Swap OR Invert off every ally
// (players + pets/Osty) and piles it onto you. Base Exhausts; upgrade keeps it (a repeatable team
// debuff-vacuum). Pure reuse of the Swap pipeline, run in reverse (ally -> self instead of self -> enemies).
public class Antagonist : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Antagonist";

    // Only obtainable/playable in co-op — it pulls from allies.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Antagonist() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove); // Exhaust base; gone upgraded
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var self = cardPlay.Card.Owner.Creature;
        // Every ally-side creature except me (players + pets/Osty).
        var allies = self.CombatState!.Allies.Where(c => (c?.IsAlive ?? false) && c != self);

        // Build one plan across all allies from a live snapshot, then execute once (removes-before-applies,
        // clone-safe, multiplayer-deterministic — see SceneStealing.ExecutePlan).
        var plan = new SceneStealing.SwapPlan();
        foreach (var ally in allies)
        {
            // "Swappable OR invertible", deduped by power Id (many overlap, e.g. Weak is both).
            var holdings = new Dictionary<ModelId, SceneStealing.DebuffHolding>();
            foreach (var h in SceneStealing.GiveableDebuffs(ally)) holdings[h.Power.Id] = h;
            foreach (var pair in InvertiblePairs.All)
                if (pair.DebuffHoldingOn(ally) is { } h) holdings[h.Power.Id] = h;

            foreach (var h in holdings.Values)
            {
                // Take the whole magnitude: remove it from the ally (holder) and land it on self (recipient).
                // The give-side helpers are direction-agnostic — passing (ally, self) reverses normal Swap.
                plan.Removes.Add(SceneStealing.RemoveDebuffFromSelf(h, ally, h.Magnitude));
                plan.Applies.Add(SceneStealing.GiveDebuffToEnemy(h, self, h.Magnitude));
            }
        }

        await SceneStealing.ExecutePlan(context, self, plan);
    }
}
