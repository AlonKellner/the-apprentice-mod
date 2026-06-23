using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ApprenticeDefend : ApprenticeCard
{
    public const string CardId = "TheApprentice:ApprenticeDefend";

    public ApprenticeDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None, false)
    {
        WithBlock(5);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
    }
}
