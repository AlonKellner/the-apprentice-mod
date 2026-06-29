using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Compose : ApprenticeCard
{
    public const string CardId = "TheApprentice:Compose";

    public Compose() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int cap = IsUpgraded ? 2 : 1;
        int weak = Math.Min((int)creature.GetPowerAmount<WeakPower>(), cap);
        if (weak > 0)
            await PowerCmd.Apply<WeakPower>(context, creature, -weak, creature, cardPlay.Card, false);
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
