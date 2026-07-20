using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Potions;

// Gain 2 Unweak.
public class ProteinPotion : UnderstudyPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<UnweakPower>() };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) =>
        await PowerCmd.Apply<UnweakPower>(choiceContext, Owner.Creature, 2, Owner.Creature, null, false);
}
