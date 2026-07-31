using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Swap ALL debuffs and Invert ALL debuffs simultaneously — the uncapped Best of Both engine
// (BestOfBoth.ResolveAllFor), the same simultaneous resolution Standing Ovation uses.
public class GlitterBomb : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[]
        {
            HoverTipFactory.FromKeyword(UnderstudyKeywords.Swap),
            HoverTipFactory.FromKeyword(UnderstudyKeywords.Invert),
        };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        await BestOfBoth.ResolveAllFor(choiceContext, Owner.Creature);
}
