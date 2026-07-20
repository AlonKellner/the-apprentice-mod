using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Apply Planned to one card from the draw pile, one in hand, and one from the discard — one
// selection per pile (skipped if that pile has no valid target). Uses the same CardSelectCmd +
// PlannedModifier.Apply pattern as Foreshadow.
public class PlannedPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Planned) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var player = Owner;
        await PlanOneFromPile(choiceContext, player, PileType.Draw);
        await PlanOneFromPile(choiceContext, player, PileType.Hand);
        await PlanOneFromPile(choiceContext, player, PileType.Discard);
    }

    private async Task PlanOneFromPile(PlayerChoiceContext ctx, Player player, PileType pileType)
    {
        var pile = pileType.GetPile(player);
        if (!pile.Cards.Any(PlannedModifier.CanApplyTo)) return;

        var prefs = new CardSelectorPrefs(
            new LocString("potions", "THEUNDERSTUDY-PLANNED_POTION.selectionPrompt"), 0, 1);

        IEnumerable<CardModel>? selected = pileType == PileType.Hand
            ? await CardSelectCmd.FromHand(ctx, player, prefs, PlannedModifier.CanApplyTo, this)
            : await CardSelectCmd.FromCombatPile(ctx, pile, player, prefs, PlannedModifier.CanApplyTo);

        if (selected == null) return;
        var combat = Owner.Creature.CombatState!;
        foreach (var card in selected)
            PlannedModifier.Apply(card, combat);
    }
}
