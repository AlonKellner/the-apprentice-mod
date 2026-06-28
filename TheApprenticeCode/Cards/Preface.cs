using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Preface : ApprenticeCard
{
    public const string CardId = "TheApprentice:Preface";

    public Preface() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(4);
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
    }
}
