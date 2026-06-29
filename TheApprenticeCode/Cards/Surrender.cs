using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Surrender : ApprenticeCard
{
    public const string CardId = "TheApprentice:Surrender";

    public Surrender() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int cap = IsUpgraded ? 2 : 1;
        int vuln = Math.Min((int)creature.GetPowerAmount<VulnerablePower>(), cap);
        if (vuln <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(context, creature, -vuln, creature, cardPlay.Card, false);
        await CreatureCmd.GainBlock(creature, vuln * 7m, ValueProp.Unpowered, null);
    }
}
