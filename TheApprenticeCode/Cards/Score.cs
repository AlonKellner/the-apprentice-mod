using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Score : ApprenticeCard
{
    public const string CardId = "TheApprentice:Score";

    public Score() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(18);
        WithTip(typeof(TensionPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 10, cardPlay.Card);
    }
}
