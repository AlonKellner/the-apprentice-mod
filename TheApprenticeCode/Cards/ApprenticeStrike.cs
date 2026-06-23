using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ApprenticeStrike : ApprenticeCard
{
    public const string CardId = "TheApprentice:ApprenticeStrike";

    public ApprenticeStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithDamage(6);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
