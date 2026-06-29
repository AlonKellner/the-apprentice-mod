using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Chorus : ApprenticeCard
{
    public const string CardId = "TheApprentice:Chorus";

    public Chorus() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(7);
        WithAmbitionTips();
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DamageCmd.Attack(cardPlay.Card.DynamicVars.Damage.BaseValue)
            .FromCard(cardPlay.Card)
            .TargetingAllOpponents(CombatState!)
            .Execute(context);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 1);
    }
}
