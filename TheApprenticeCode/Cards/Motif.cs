using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Motif : ApprenticeCard
{
    public const string CardId = "TheApprentice:Motif";

    public Motif() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(9);
        WithTip(typeof(TensionPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 4, cardPlay.Card);
    }
}
