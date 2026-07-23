using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "Every 5 debuffs applied to you, apply 2 Vulnerable to a random enemy." — counter relic.
// AfterPowerAmountChanged already carries a PlayerChoiceContext, so this counts and fires inline.
// Counts every debuff application landing on you, including the deck's own self-applied debuffs
// (tunable). Applying Vulnerable to an enemy is a debuff on the enemy (power.Owner != us), so it
// never re-triggers this counter.
public class Greasepaint : UnderstudyCounterRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    protected override int Threshold => 5;


    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<VulnerablePower>() };

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!power.IsMutable) return;
        if (power.Owner != Owner.Creature) return;
        if (!EmotionalExpression.IsDebuffApplication(power, amount)) return;

        int fires = Bump();
        for (int i = 0; i < fires; i++)
            await ApplyToRandomEnemy<VulnerablePower>(choiceContext, 2);
    }
}
