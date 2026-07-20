using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Apply Tuned to up to 3 chosen cards in hand (Take Notes' selection + TunedModifier.Apply pattern).
public class TunedPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Tuned) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var player = Owner;
        var prefs = new CardSelectorPrefs(
            new LocString("potions", "THEUNDERSTUDY-TUNED_POTION.selectionPrompt"), 0, 3);
        var selected = await CardSelectCmd.FromHand(choiceContext, player, prefs, TunedModifier.CanApplyTo, this);
        if (selected == null) return;

        var allCards = player.Piles.SelectMany(p => p.Cards);
        var combat = Owner.Creature.CombatState!;
        foreach (var card in selected)
            TunedModifier.Apply(card, combat, allCards);
    }
}
