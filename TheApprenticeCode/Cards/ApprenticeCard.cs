using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using TheApprentice.TheApprenticeCode.Character;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TheApprenticeCardPool))]
public abstract class ApprenticeCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true)
    : ConstructedCardModel(cost, type, rarity, target, showInCardLibrary)
{
    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    public virtual bool IsPrePlanned => false;

    public virtual bool HasExpend => false;
    public virtual bool ExpendRemovedOnUpgrade => false;

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner) return;
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    // AfterCardEnteredCombat is NOT fired for the initial deck-to-pile deal at the start of
    // combat: the engine's pile-add path explicitly early-returns ("if (newPile.IsCombatPile &&
    // !CombatManager.Instance.IsInProgress) return;") while combat is still starting, before
    // IsInProgress flips true. It only fires for cards entering a combat pile *after* combat is
    // already in progress (card generation/transformation mid-fight).
    //
    // BeforeCombatStart is the right hook for "starts each combat" effects: it's dispatched via
    // RunState.IterateHookListeners, which walks every card in the player's deck directly
    // (independent of piles), and the engine calls it (StartCombatInternal) after
    // PopulateCombatState has already shuffled the deck into piles and after IsInProgress is set
    // true — so CombatState/Owner are valid by the time it fires.
    //
    // Both hooks call the same logic and are individually idempotent (guarded by
    // TryGetModifier), so wiring both is safe: BeforeCombatStart covers the cards present at
    // combat start, AfterCardEnteredCombat covers the rare case of one of our cards being
    // generated into a pile mid-combat.
    public override Task BeforeCombatStart()
    {
        ApplyCombatStartModifiers();
        return Task.CompletedTask;
    }

    // Must live on the card itself, not on TheApprenticeCardPool: CardPoolModel.ShouldReceiveCombatHooks
    // defaults to false and the pool never opts in via ModHelper.SubscribeForCombatStateHooks, so a
    // pool-level override of this hook is never actually invoked by the engine. This hook fires on
    // every listener for every card-entered event, so it must self-filter.
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return Task.CompletedTask;
        ApplyCombatStartModifiers();
        return Task.CompletedTask;
    }

    private void ApplyCombatStartModifiers()
    {
        // BeforeCombatStart/AfterCardEnteredCombat fire on BOTH the persistent Deck-pile card
        // AND its combat-pile clone: RunState.IterateHookListeners(combatState) walks
        // player.Deck.Cards directly *in addition to* combat-pile cards, because PileType.Deck's
        // own doc comment says cards there "live between rooms" and get "cloned into your draw
        // pile" when combat starts — two separate CardModel instances for what looks like one
        // card. Attaching to the inert Deck original is worse than pointless: PlannedCounterPower
        // (and DisplayAmount generally) enumerate Player.Piles, which includes the Deck pile, so
        // a card with both a Deck original and a Draw/Hand clone — e.g. two Signature copies —
        // shows up as four Planned entries instead of two. Only attach to the pile-clone that's
        // actually played in this combat.
        bool inCombatPile = Pile?.Type.IsCombatPile() == true;

        // CardModifier.AddModifier<T>(card) (generic) — NOT `new T()` — is required here: the
        // real game registers a canonical instance of every CardModifier at boot, and directly
        // constructing a second one throws DuplicateModelException ("Don't call constructors on
        // models! Use ModelDb instead."). See commit "Fix DuplicateModelException when playing
        // Aching Wish" for the same lesson learned with DreamyModifier/AmbitousModifier/ExpendModifier.
        if (IsPrePlanned && inCombatPile && !this.TryGetModifier<PlannedModifier>(out _))
        {
            CardModifier.AddModifier<PlannedModifier>(this);
            if (this.TryGetModifier<PlannedModifier>(out var mod))
                mod.SequenceIndices.Add(-1);
            // Every other place that changes Planned state (PlannedModifier.Apply, TabulaRasa,
            // Transpose, JustAsPlanned) calls this afterwards — PlannedCounterPower is the only
            // subscriber, so this keeps its card-list description in sync once it exists.
            PlannedModifier.InvokeChanged();
            // PlannedCounterPower doesn't exist yet this early — it's only applied later, at
            // turn start (AfterPlayerTurnStartLate above) — so nothing is subscribed to Changed
            // and VisualIndex is never computed from the InvokeChanged() call alone. UI that can
            // preview a card before turn start (e.g. Constant Struggle's draw-pile selection
            // prompt) would otherwise show every pre-Planned card as "Planned #0"
            // (SequenceIndex(-1) + 1) instead of its real position. Recompute it directly here too.
            PlannedModifier.RefreshVisualIndices(PlannedModifier.RelevantCards(Owner));
        }
        if (HasExpend && !(ExpendRemovedOnUpgrade && IsUpgraded))
        {
            if (!this.TryGetModifier<ExpendModifier>(out var expendMod))
            {
                if (inCombatPile)
                    CardModifier.AddModifier<ExpendModifier>(this);
            }
            else
                expendMod.Reset();
        }
    }

    protected void WithDreamKeywordTips()
    {
        WithTip(ApprenticeKeywords.Dreamy);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithAmbitionKeywordTips()
    {
        WithTip(ApprenticeKeywords.Ambitous);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithPotentialKeywordTips()
    {
        WithTip(ApprenticeKeywords.Ambitous);
        WithTip(ApprenticeKeywords.Dreamy);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithDreamTips()
    {
        WithTip(typeof(Dream));
        WithDreamKeywordTips();
    }

    protected void WithAmbitionTips()
    {
        WithTip(typeof(Ambition));
        WithAmbitionKeywordTips();
    }

    protected void WithPotentialTips()
    {
        WithTip(typeof(Potential));
        WithPotentialKeywordTips();
    }
}
