using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// "Invert 3" — invert up to 3 of each of your invertible debuffs (EmotionalExpression.InvertEach).
public class InvertPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Invert) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        await EmotionalExpression.InvertEach(choiceContext, Owner.Creature, 3);
}
