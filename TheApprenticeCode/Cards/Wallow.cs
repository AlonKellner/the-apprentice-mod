using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Wallow : ApprenticeCard
{
    public const string CardId = "TheApprentice:Wallow";

    public Wallow() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(WeakPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int weak = (int)creature.GetPowerAmount<WeakPower>();
        if (weak <= 0) return;
        int multiplier = IsUpgraded ? 6 : 4;
        await CreatureCmd.GainBlock(creature, weak * multiplier, ValueProp.Unpowered, null);
    }
}
