using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Cadenza : ApprenticeCard
{
    public const string CardId = "TheApprentice:Cadenza";

    protected override bool HasEnergyCostX => true;

    public Cadenza() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(6);
        WithTip(typeof(TensionPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (x > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay, x).Execute(context);
        if (x > 0)
            await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, x, cardPlay.Card);
    }
}
