using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Remove Unplayable from every attack/skill across your whole combat deck — draw, hand and discard
// (Composure's mechanism, deck-wide).
public class Milkshake : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(CardKeyword.Unplayable) };

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var cards = Owner.Piles.Where(p => p.Type.IsCombatPile())
            .SelectMany(p => p.Cards).Where(UnplayableModifier.CanApplyTo).ToList();
        foreach (var card in cards)
            UnplayableModifier.Remove(card);
        return Task.CompletedTask;
    }
}
