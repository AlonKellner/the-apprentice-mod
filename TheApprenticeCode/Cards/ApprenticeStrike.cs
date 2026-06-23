using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ApprenticeStrike : ApprenticeCard
{
    public const string CardId = "TheApprentice:ApprenticeStrike";

    public ApprenticeStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await DamageCmd.Attack(IsUpgraded ? 9m : 6m)
            .FromCard(cardPlay.Card)
            .Targeting(cardPlay.Target!)
            .Execute(context);
    }
}
