using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Overthinking : ApprenticeCard
{
    public const string CardId = "TheApprentice:Overthinking";

    public Overthinking() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(4);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int count = player.Piles.SelectMany(p => p.Cards).Count(c => c.IsUnplayable());
        for (int i = 0; i < count; i++)
            await CommonActions.CardBlock(cardPlay.Card, cardPlay);
    }
}
