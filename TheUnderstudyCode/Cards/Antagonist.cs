using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The understudy who volunteers to be the villain: a full TRADE run through the Swap pipeline with the roles
// reversed and allies as the counterparty. A regular Swap gives YOUR debuffs to enemies and takes THEIR
// buffs; Antagonist gives YOUR invertible/swappable BUFFS to every ally and takes THEIR invertible/swappable
// DEBUFFS onto yourself — buffs and debuffs swap which side gives and which takes. Base Exhausts; upgrade
// keeps it (a repeatable team debuff-vacuum + buff-donor). Single capture -> remove -> apply plan, so the
// give and take resolve against one live snapshot (interacting powers move instead of cancelling).
public class Antagonist : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Antagonist";

    // Only obtainable/playable in co-op — it trades with allies.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Antagonist() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove); // Exhaust base; gone upgraded
        WithTip(UnderstudyKeywords.Invertible);
        WithTip(UnderstudyKeywords.Swappable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var self = cardPlay.Card.Owner.Creature;
        // Every ally-side creature except me (players + pets/Osty).
        var allies = self.CombatState!.Allies.Where(c => (c?.IsAlive ?? false) && c != self).ToList();

        // One plan, one live snapshot: build every move first, then execute (removes-before-applies,
        // clone-safe, multiplayer-deterministic — see SceneStealing.ExecutePlan).
        var plan = new SceneStealing.SwapPlan();

        // GIVE (roles reversed from Swap): hand each of your invertible/swappable BUFFS to every ally. Remove
        // it from yourself once and grant its full magnitude to each ally, mirroring how Swap's give lands a
        // debuff on all enemies. "Swappable OR invertible", deduped by power Id (many overlap, e.g. Unweak).
        var myBuffs = new Dictionary<ModelId, SceneStealing.BuffHolding>();
        foreach (var b in SceneStealing.GiveableBuffs(self)) myBuffs[b.Power.Id] = b;
        foreach (var pair in InvertiblePairs.All)
            if (pair.BuffHoldingOn(self) is { } b) myBuffs[b.Power.Id] = b;
        foreach (var b in myBuffs.Values)
        {
            plan.Removes.Add(new SceneStealing.PowerMove(b.Power, self, -b.Magnitude));
            foreach (var ally in allies) plan.Applies.Add(new SceneStealing.PowerMove(b.Power, ally, b.Magnitude));
        }

        // TAKE: pull every invertible/swappable DEBUFF off each ally and pile its full magnitude onto self.
        // The give-side helpers are direction-agnostic — passing (ally, self) reverses normal Swap's take.
        foreach (var ally in allies)
        {
            var holdings = new Dictionary<ModelId, SceneStealing.DebuffHolding>();
            foreach (var h in SceneStealing.GiveableDebuffs(ally)) holdings[h.Power.Id] = h;
            foreach (var pair in InvertiblePairs.All)
                if (pair.DebuffHoldingOn(ally) is { } h) holdings[h.Power.Id] = h;

            foreach (var h in holdings.Values)
            {
                plan.Removes.Add(SceneStealing.RemoveDebuffFromSelf(h, ally, h.Magnitude));
                plan.Applies.Add(SceneStealing.GiveDebuffToEnemy(h, self, h.Magnitude));
            }
        }

        await SceneStealing.ExecutePlan(context, self, plan);
    }
}
