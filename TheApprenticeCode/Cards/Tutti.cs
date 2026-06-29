using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tutti : ApprenticeCard
{
    public const string CardId = "TheApprentice:Tutti";

    public Tutti() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(5);
        WithTip(typeof(TensionPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await DamageCmd.Attack(cardPlay.Card.DynamicVars.Damage.BaseValue)
            .FromCard(cardPlay.Card)
            .TargetingAllOpponents(CombatState!)
            .Execute(context);
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 5, cardPlay.Card);
    }
}
