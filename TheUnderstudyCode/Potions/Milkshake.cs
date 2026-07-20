using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Remove Unplayable from every attack/skill in hand (Composure's mechanism).
public class Milkshake : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(CardKeyword.Unplayable) };

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        foreach (var card in PileType.Hand.GetPile(Owner).Cards.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
        return Task.CompletedTask;
    }
}
