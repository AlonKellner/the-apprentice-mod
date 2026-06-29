using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Brace : ApprenticeCard
{
    public const string CardId = "TheApprentice:Brace";

    public Brace() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        decimal strength = creature.GetPowerAmount<StrengthPower>();
        if (strength >= 0) return;
        int multiplier = IsUpgraded ? 6 : 4;
        await CreatureCmd.GainBlock(creature, (int)Math.Abs(strength) * multiplier, ValueProp.Unpowered, null);
    }
}
