using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Burden : ApprenticeCard
{
    public const string CardId = "TheApprentice:Burden";

    public Burden() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithTip(typeof(StrengthPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int cap = IsUpgraded ? 3 : 2;
        decimal strength = creature.GetPowerAmount<StrengthPower>();
        int negStrength = (int)Math.Min(Math.Abs(Math.Min(strength, 0)), cap);
        if (negStrength > 0)
            await PowerCmd.Apply<StrengthPower>(context, creature, negStrength, creature, cardPlay.Card, false);
        if (negStrength > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, negStrength * 12m).Execute(context);
    }
}
