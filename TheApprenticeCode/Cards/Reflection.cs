using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Reflection : ApprenticeCard
{
    public const string CardId = "TheApprentice:Reflection";

    public Reflection() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(WeakPower));
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int weakAmount = creature.GetPowerAmount<WeakPower>();
        if (weakAmount <= 0) return;

        int strengthGain = weakAmount + (IsUpgraded ? 1 : 0);
        await PowerCmd.Apply<WeakPower>(context, creature, -weakAmount, creature, cardPlay.Card, false);
        await PowerCmd.Apply<StrengthPower>(context, creature, strengthGain, creature, cardPlay.Card, false);
    }
}
