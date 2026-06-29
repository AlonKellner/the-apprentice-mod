using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Marcato : ApprenticeCard
{
    public const string CardId = "TheApprentice:Marcato";

    public Marcato() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(3);
        WithTip(typeof(TensionPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        decimal dmg = cardPlay.Card.DynamicVars.Damage.BaseValue;
        for (int i = 0; i < 4; i++)
        {
            await DamageCmd.Attack(dmg)
                .FromCard(cardPlay.Card)
                .TargetingRandomOpponents(CombatState!)
                .Execute(context);
        }
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 5, cardPlay.Card);
    }
}
