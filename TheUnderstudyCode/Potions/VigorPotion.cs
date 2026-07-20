using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Gain 10 Vigor.
public class VigorPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<VigorPower>() };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, 10, Owner.Creature, null, false);
}
