using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class CreativeBlock : ApprenticeCard
{
    public const string CardId = "TheApprentice:CreativeBlock";

    public CreativeBlock() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithBlock(8);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);

        bool hasNoPlanned = !PlannedModifier.AnyIn(player.Piles.SelectMany(p => p.Cards));
        if (hasNoPlanned)
            await CommonActions.CardBlock(cardPlay.Card, cardPlay);
    }
}
