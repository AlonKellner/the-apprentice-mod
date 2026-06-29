using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Prelude : ApprenticeCard
{
    public const string CardId = "TheApprentice:Prelude";

    public override bool IsPrePlanned => true;

    public Prelude() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(10);
        WithCards(2);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await CommonActions.Draw(this, context);
    }
}
