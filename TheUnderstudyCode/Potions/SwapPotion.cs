using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// "Swap 3 times" — Swap is a repeat count now, not a magnitude (SceneStealing.Swap).
public class SwapPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Swap) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        await SceneStealing.Swap(choiceContext, Owner.Creature, 3);
}
